using System;
using System.Collections.Generic;

namespace LibCommon.Assets
{
  public abstract class AssetStore
  {
    private readonly Dictionary<string, object> _assetCache = new Dictionary<string, object>();

    private static AssetStore _instance;

    #region Static public methods

    public static void SetInstance(AssetStore instance)
    {
      _instance = instance;
    }

    public static bool GetAsset(string guid, out object asset)
    {
      if (_instance != null)
      {
        return _instance.GetAssetInternal(guid, out asset);
      }
      asset = null;
      return false;
    }

    public static bool IsAssetLoaded(string guid)
    {
      if (_instance != null)
      {
        return _instance.IsAssetLoadedInternal(guid);
      }
      return false;
    }

    public static void LoadAsset(string guid, Action onLoadCompleteCallback)
    {
      _instance?.LoadAssetInternal(guid, onLoadCompleteCallback);
    }

    public static void LoadBulkAsset(string bundleName, Action onLoadCompleteCallback)
    {
      _instance?.LoadBulkAssetInternal(bundleName, onLoadCompleteCallback);
    }

    #endregion

    #region Instance Methods

    private bool IsAssetLoadedInternal(string guid)
    {
      return _assetCache.ContainsKey(guid);
    }

    protected abstract void LoadAssetInternal(string guid, Action onLoadCompleteCallback);

    public abstract void LoadBulkAssetInternal(string bundleName, Action onLoadCompleteCallback);

    protected bool GetAssetInternal(string guid, out object asset)
    {
      bool success = _assetCache.TryGetValue(guid, out asset);
      return success;
    }

    protected void AddAsset(string guid, object asset)
    {
      _assetCache[guid] = asset;
    }

    #endregion
  }
}
