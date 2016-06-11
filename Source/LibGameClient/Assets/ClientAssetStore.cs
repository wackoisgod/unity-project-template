using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon.Assets;
using LibCommon.Manager;
using Object = UnityEngine.Object;

namespace LibGameClient.Data
{
    public class ClientAssetStore : AssetStore
    {
        protected override void LoadAssetInternal(string guid, Action onLoadCompleteCallback)
        {
            if (IsAssetLoaded(guid))
            {
                onLoadCompleteCallback?.Invoke();
                return;
            }

            //Use the manifest to pull the url
            AssetBundleLoader request = AssetBundleLoader.FromGUID(guid);
            if (request == null) return;

            request.OnCompleteLoading += asset =>
            {
                AssetBundleLoader assetBundleLoader = asset as AssetBundleLoader;
                Object assetBundle = assetBundleLoader?.AssetObject;
                if (assetBundle == null) return;

                UnityEngine.Debug.Log("adding asset " + assetBundleLoader.AssetName + " guid" + guid);

                AddAsset(guid, assetBundle);

                onLoadCompleteCallback();
            };

            AssetManager.Instance.RequestAssetLoad(request);
        }

        public override void LoadBulkAssetInternal(string bundleName, Action onLoadCompleteCallback)
        {
            BulkAssetBundleLoader request = BulkAssetBundleLoader.FromGUIDS(bundleName);

            if (request == null) return;

            request.OnCompleteLoading += asset =>
            {
                BulkAssetBundleLoader bulkAssetBundleLoader = asset as BulkAssetBundleLoader;
                if (bulkAssetBundleLoader == null) return;

                List<Object> assetBundle = bulkAssetBundleLoader.AssetObjects.ToList();
                List<string> assetNames = bulkAssetBundleLoader.AssetNames.ToList();
                List<string> assetGUIDS = bulkAssetBundleLoader.AssetGUIDs.ToList();

                foreach (var item in assetBundle)
                {
                    UnityEngine.Debug.Log("looking for " + item.name);

                    int index = assetNames.FindIndex(x => x.Contains(item.name));
                    if (index == -1) continue;

                    string guid = assetGUIDS[index];
                    UnityEngine.Debug.Log("adding asset " + assetNames[index] + " guid" + guid);
                    AddAsset(guid, item);
                }
                onLoadCompleteCallback();
            };

            AssetManager.Instance.RequestAssetLoad(request);
        }
    }
}
