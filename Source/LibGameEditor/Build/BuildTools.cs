using System;
using System.Collections.Generic;
using System.Linq;
using LibGameEditor.Build.Bundles;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Build
{
  internal class BuildTools
  {
    public static string[] GetScenes()
    {
      IEnumerable<string> scenes = from scene in EditorBuildSettings.scenes
        where scene.enabled
        select scene.path;

      return scenes.ToArray();
    }

    [MenuItem("Build/Standalone/Windows Player/Client")]
    public static void BuildWindowsStandalonePlayer()
    {
      Build(GetScenes(), BuildTarget.StandaloneWindows, "/StandaloneWindows/Client/", ".exe");
    }

    [MenuItem("Build/Standalone/Windows Player/Assets")]
    public static void BuildWindowsStandaloneAssets()
    {
      AssetBundler.BuildAssetBundles(Application.dataPath + "/../../Build/StandaloneWindows/Assets/",
        BuildTarget.StandaloneWindows);
    }

    private static void Build(string[] levels, BuildTarget buildTarget, string deployPath, string ext, bool debug = true)
    {
      BuildOptions buildOptions = debug ? BuildOptions.AllowDebugging | BuildOptions.Development : BuildOptions.None;

      const string productName = "goblin";

      try
      {
        PlayerSettings.bundleVersion = "0." + Environment.GetEnvironmentVariable("P4_CHANGELIST");
      }
      catch (Exception)
      {
        PlayerSettings.bundleVersion = "0xd3adb33f";
      }

      string buildPath = Application.dataPath + "/../../Build" + deployPath + productName.Replace(" ", "") + ext;

      EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
      BuildPipeline.BuildPlayer(levels, buildPath, buildTarget, buildOptions);
    }
  }
}
