using System;
using LibCommon.Assets;
using UnityEditor;

namespace LibGameEditor.Assets
{
  public class EditorAssetStore : AssetStore
  {
    protected override void LoadAssetInternal(string guid, Action onAssetLoadedCallback)
    {
      if (!IsAssetLoaded(guid))
      {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        object asset = AssetDatabase.LoadMainAssetAtPath(path);
        AddAsset(guid, asset);
      }
      onAssetLoadedCallback();
    }

    public override void LoadBulkAssetInternal(string bundleName, Action onLoadCompleteCallback)
    {
      onLoadCompleteCallback();
    }
  }
}
