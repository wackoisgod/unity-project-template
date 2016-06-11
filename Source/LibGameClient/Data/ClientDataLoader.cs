using System;
using System.IO;
using LibCommon.Data;
using LibCommon.Manager;
using LibCommon.Manager.Assets;
using LibGameClient.Data.Assets;
using UnityEngine;

namespace LibGameClient.Data
{
  namespace Assets
  {
    public class XMLAssetLoader : AssetLoadRequest
    {
      private readonly Type _dataType;
      private readonly string _pathName;
      private readonly byte[] _bytes;

      private readonly bool _useBytes;

      public object Content { get; set; }

      public XMLAssetLoader(Type inType, string pathName)
      {
        _pathName = pathName;
        _dataType = inType;
      }

      public XMLAssetLoader(Type inType, byte[] inBytes)
      {
        _bytes = inBytes;
        _useBytes = true;
        _dataType = inType;
      }

      public override void Load()
      {
        try
        {
          Content = !_useBytes ? Serializer.Deserialize(_dataType, _pathName) : Serializer.Deserialize(_dataType, _bytes);
          if (Content == null)
          {
            HasFailed = true;
          }

          IsDone = true;
        }
        catch (Exception)
        {
          IsDone = true;
          HasFailed = true;
        }
      }
    }
  }

  public class ClientDataLoader : DataLoader
  {
    public static string DataPath
    {
      get
      {
        // FUCK YOU!
        if (Application.isEditor)
          return Application.dataPath + "/Data/CombinedData.xml";
        return Application.persistentDataPath + "/Data/CombinedData.xml";
      }
    }

    public Action<int> OnDataLoadComplete;

    public override void PopulateDataStore()
    {
      // if we are not using the web for data then we can just set this up different
      {
        LoadDataPath(DataPath, typeof(CombinedData));
      }
    }

    private void LoadDataPath(string location, Type inType)
    {
      try
      {
        XMLAssetLoader loader = new XMLAssetLoader(inType, location);
        loader.OnCompleteLoading += OnXMLLoadComplete;
        loader.OnFailedLoading += OnXMLFailedLoad;
        AssetManager.Instance.RequestAssetLoad(loader);
      }
      catch (DirectoryNotFoundException)
      {
        // log an error here ? failed to load some file 
      }
    }

    private void LoadDataPath(byte[] location, Type inType)
    {
      try
      {
        XMLAssetLoader loader = new XMLAssetLoader(inType, location);
        loader.OnCompleteLoading += OnXMLLoadComplete;
        loader.OnFailedLoading += OnXMLFailedLoad;
        AssetManager.Instance.RequestAssetLoad(loader);
      }
      catch (DirectoryNotFoundException)
      {
        // log an error here ? failed to load some file 
      }
    }

    public void OnXMLLoadComplete(AssetLoadRequest inValue)
    {
      var xmlAssetLoader = inValue as XMLAssetLoader;
      CombinedData data = xmlAssetLoader?.Content as CombinedData;
      if (data != null)
      {
        foreach (BaseData instance in data.Data)
        {
          DataStore.AddData(instance);
        }
      }

      OnLoadingComplete(0);
    }

    public void OnXMLFailedLoad(AssetLoadRequest inValue)
    {
    }

    public void OnLoadingComplete(int failCount)
    {
      OnDataLoadComplete?.Invoke(failCount);
    }
  }
}
