using LibCommon.Assets;
using LibCommon.Data;
using LibGameEditor.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace LibGameEditor.Build.Bundles
{
  public class AssetBundler
  {
    // holder class that makes things easier for specifcally named bundles
    public class Bundle
    {
      public string Name;
      public List<string> Guids = new List<string>();
      public List<string> AssetPaths = new List<string>();
      public List<string> Dependecies = new List<string>();

      public Bundle (string inName, List<string> inGUIDs, List<string> inAssetPaths)
      {
        Name = inName;
        Guids = inGUIDs;
        AssetPaths = inAssetPaths;
        Dependecies = new List<string>();
      }

      public Bundle()
      {
      }
    }

    // Grabs the CRC from the Bundle Manifest file
    public static string GetFileCRC(string inFileName)
    {
      using (StreamReader inputReader = new StreamReader(inFileName))
      {
        while (!inputReader.EndOfStream)
        {
          var line = inputReader.ReadLine();

          // find the CRC :P 
          if (line.Contains("CRC"))
          {
            var values = line.Split(new char[] { ' ' });
            return values[1];
          }
        }
      }

      return string.Empty;
    }

    public static void BuildAssetBundles(string inOutputDir, BuildTarget inBuildTarget)
    {
      // This will force the AssetDB to pick up any changes 
      AssetDatabase.Refresh();

      List<AssetManifest.BundleInfo> bundleInfoList = new List<AssetManifest.BundleInfo>();
      List<AssetBundleBuild> bundleList = new List<AssetBundleBuild>();

      Dictionary<string, Bundle> dataBundles = new Dictionary<string, Bundle>();

      EditorDataUtil.ProcessAcrossAllData((EditorDataUtil.DataInfo info) =>
      {
        string bundleName = info.Data.Name + info.Data.GetType().Name;

        List<string> guids = new List<string>();
        List<string> assetPaths = new List<string>();

        // we always have the base bundle at index 0
        dataBundles[bundleName] = new Bundle(bundleName, guids, assetPaths);

        ExtractBundleReference(bundleName, info.Data, guids, assetPaths, ref dataBundles);
      });

      foreach (var b in dataBundles)
      {
        Bundle currentBundle = b.Value;

        if (currentBundle.Guids.Count > 0)
        {
          AssetBundleBuild bundle = new AssetBundleBuild();
          bundle.assetBundleName = b.Key;
          bundle.assetNames = currentBundle.AssetPaths.Distinct().ToArray();
          bundleList.Add(bundle);

          AssetManifest.BundleInfo bundleInfo = new AssetManifest.BundleInfo();
          bundleInfo.assetNames = bundle.assetNames;
          bundleInfo.guids = currentBundle.Guids.Distinct().ToArray();
          bundleInfo.name = b.Key.ToLower(); // the files are case sensitive on the web under unix systems! Thus we use lowercase
          bundleInfo.dependencies = currentBundle.Dependecies.ToArray();
          bundleInfoList.Add(bundleInfo);
        }
      }

      if (!Directory.Exists(inOutputDir))
      {
        Directory.CreateDirectory(inOutputDir);
      }

      // Uncompress bundles on android are a must due to the slow IO :( 
      // outside that we should always used the chunk based compression.
      BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle;
      if (inBuildTarget != BuildTarget.Android)
        options = BuildAssetBundleOptions.ChunkBasedCompression;

      var unityManifestFile = BuildPipeline.BuildAssetBundles(inOutputDir, bundleList.ToArray(), options, inBuildTarget);

      // no new bundles were generated during this build.
      if (unityManifestFile == null) return;

      string[] files = Directory.GetFiles(inOutputDir, "*.manifest");
      AssetManifest manifest = new AssetManifest();

      manifest.bundles = bundleInfoList.ToArray();
      foreach (AssetManifest.BundleInfo bundleInfo in manifest.bundles)
      {
        var hash = string.Empty;
        var bundlePath = files.Where(x => Path.GetFileNameWithoutExtension(x) == bundleInfo.name).FirstOrDefault();
        if (!string.IsNullOrEmpty(bundlePath))
        {
          hash = GetFileCRC(bundlePath);
        }

        // we use the hash included in the bundle manifest so we can then use it for versioning
        // when passing into LoadFromCacheOrDownload ? 
        bundleInfo.hash = hash;
        string[] dependencies = unityManifestFile.GetAllDependencies(bundleInfo.name);

        // messy but oh well :P 
        bundleInfo.dependencies = bundleInfo.dependencies.Concat(dependencies).Distinct().ToArray();
      }

      // Set some version number! 
      int version = 0;
      manifest.version = version;

      Serializer.Serialize<AssetManifest>(manifest, inOutputDir + Path.DirectorySeparatorChar + "AssetManifest.xml");
    }

    private static void ExtractBundleReference(string inDataBundleName, object target, List<string> guids, List<string> assetPaths, ref Dictionary<string, Bundle> dataBundles)
    {
      PropertyInfo[] properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var referenceProperties = from property in properties
                                where (property.GetCustomAttributes(typeof(AssetReferenceAttribute), true).Length > 0)
                                select property;

      foreach (PropertyInfo property in referenceProperties)
      {
       
        if (property.PropertyType == typeof(string))
        {
          string guid = property.GetValue(target, null) as string;
          if (!string.IsNullOrEmpty(guid))
          {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
              AssetReferenceAttribute attribute = property.GetCustomAttributes(typeof(AssetReferenceAttribute), true)[0] as AssetReferenceAttribute;
              bool customBundle = (!string.IsNullOrEmpty(attribute.BundleOverride));

              Bundle currentBundle;
              if (!customBundle)
              {
                currentBundle = dataBundles[inDataBundleName];
                guids.Add(guid);
                assetPaths.Add(path);
              }
              else
              {
                if (dataBundles.ContainsKey(attribute.BundleOverride))
                {
                  currentBundle = dataBundles[attribute.BundleOverride];
                }
                else
                {
                  dataBundles[attribute.BundleOverride] = currentBundle = new Bundle(attribute.BundleOverride, new List<string>(), new List<string>());
                }
              }

              currentBundle.Guids.Add(guid);
              currentBundle.AssetPaths.Add(path);
              if (!string.IsNullOrEmpty(attribute.DependentBundle))
                currentBundle.Dependecies.Add(attribute.DependentBundle.ToLower());
            }
          }
        }
      }

      foreach (PropertyInfo property in properties)
      {
        if (!property.PropertyType.IsPrimitive)
        {
          object subTarget = property.GetValue(target, null);
          if (subTarget != null)
          {
            if (subTarget.GetType().IsArray)
            {
              Array array = (Array)subTarget;
              for (int i = 0; i < array.Length; i++)
              {
                ExtractBundleReference(inDataBundleName, array.GetValue(i), guids, assetPaths, ref dataBundles);
              }
            }
            else
            {
              ExtractBundleReference(inDataBundleName, subTarget, guids, assetPaths, ref dataBundles);
            }
          }
        }
      }
    }
  }
}
