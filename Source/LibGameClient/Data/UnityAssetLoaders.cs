using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LibCommon.Assets;
using LibCommon.Manager;
using LibCommon.Utils;
using LibGameClient.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LibGameClient.Data
{
    public static class UnityAssetUtils
    {
        public static string GetAssetURL(string inAsset)
        {
            string baseURL = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    baseURL += "Android/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    baseURL += "IOS/";
                    break;
                case RuntimePlatform.LinuxPlayer:
                    baseURL += "Linux/";
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    baseURL += "OSX/";
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    baseURL += "StandAlone/";
                    break;
                default:
                    baseURL += "StandAlone/";
                    break;
            }

            baseURL += inAsset;

            return baseURL;
        }
    }

    public class AsyncSceneLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public string SceneName { get; set; }

        public AsyncSceneLoader(string inSceneName)
        {
            SceneName = inSceneName;
            UseCustomFunction = true;
        }

        public override void Load()
        {
            GameManager.Instance.StartCoroutine(LoadLevel());
        }

        public IEnumerator LoadLevel()
        {
            var asyncLoad = SceneManager.LoadSceneAsync(SceneName);
            yield return asyncLoad;

            IsDone = true;

            OnDone?.Invoke();
        }
    }

    public class AssetBundleLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public bool DeferredRelease;

        public static Dictionary<string, Queue<AssetBundleLoader>> PendingBundleRequest =
            new Dictionary<string, Queue<AssetBundleLoader>>();

        public static void OnBundleDownloaded(bool inValue, UnityAssetLoader.AsyncBundleWebRequest request)
        {
            if (!inValue)
            {
                return;
            }

            bool isReleaseDeferred = false;

            if (PendingBundleRequest.ContainsKey(request.BundleName))
            {
                Queue<AssetBundleLoader> pendingRequest = PendingBundleRequest[request.BundleName];

                if (pendingRequest != null)
                {
                    while (pendingRequest.Count > 0)
                    {
                        var r = pendingRequest.Dequeue();
                        r.OnCompleteDownload(true, request.AssetBundle);
                        if (r.DeferredRelease)
                        {
                            r._bundleUnloadCallback = () =>
                            {
                                if (request.AssetBundle != null)
                                {
                                    request.AssetBundle.Unload(false);
                                    request.AssetBundle = null;
                                }
                            };
                            isReleaseDeferred = true;
                        }
                    }
                }

                PendingBundleRequest[request.BundleName].Clear();
                PendingBundleRequest.Remove(request.BundleName);
            }

            if (!isReleaseDeferred
                && request.AssetBundle != null)
            {
                request.AssetBundle.Unload(false);
                request.AssetBundle = null;
            }
        }

        public string AssetGUID { get; set; }
        public string AssetName { get; set; }
        public string BundleName { get; set; }
        public UnityEngine.Object AssetObject { get; set; }
        private Action _bundleUnloadCallback;
        private Action _dependencyMainHook;

        public AssetBundleLoader(string inGUID)
        {
            AssetGUID = inGUID;
            UseCustomFunction = true;
        }

        public static AssetBundleLoader FromGUID(string guid)
        {
            AssetBundleLoader loader = new AssetBundleLoader(guid);
            return loader;
        }

        public override void Load()
        {
            var bundleData = AssetManager.Instance.GetBundleInfoByGUID(AssetGUID);

            if (bundleData == null)
            {
                IsDone = true;
                HasFailed = true;
                OnDone?.Invoke();
                return;
            }

            // we need to find the assetName ? or do we just load all assets ?
            int index = Array.FindIndex(bundleData.guids, g => g == AssetGUID);
            AssetName = bundleData.assetNames[index]; // this mapping should always be 1-1
            BundleName = bundleData.name;

            // if we find the bundle is in flight then we can just piggy back off its completion!
            if (PendingBundleRequest.ContainsKey(BundleName))
            {
                PendingBundleRequest[BundleName].Enqueue(this);
            }
            else
            {
                PendingBundleRequest[BundleName] = new Queue<AssetBundleLoader>();
                PendingBundleRequest[BundleName].Enqueue(this);

                Action loadFunc = () =>
                {
                    var request = new UnityAssetLoader.AsyncBundleWebRequest();
                    request.URL = UnityAssetUtils.GetAssetURL(BundleName);
                    request.BundleHash = bundleData.hash;
                    request.OnCompleteDownload += OnBundleDownloaded;
                    request.BundleName = BundleName;


                    GameManager.Instance.StartCoroutine(request.DownloadRequest());
                };
                if (bundleData.dependencies == null
                    || bundleData.dependencies.Length == 0)
                {
                    loadFunc();
                }
                else
                {
                    List<AssetBundleLoader> dependencyLoaders = new List<AssetBundleLoader>();
                    int dependencyCount = bundleData.dependencies.Length;
                    _dependencyMainHook += () =>
                    {
                        for (int k = 0; k < dependencyLoaders.Count; k++)
                        {
                            dependencyLoaders[k].UnloadBundle();
                        }
                    };
                    for (int i = 0; i < bundleData.dependencies.Length; i++)
                    {
                        var dependencyBundle = AssetManager.Instance.GetBundleInfoByName(bundleData.dependencies[i]);
                        string dependencyGUID = dependencyBundle.guids[0];

                        AssetBundleLoader assetLoader = FromGUID(dependencyGUID);
                        assetLoader.DeferredRelease = true;
                        dependencyLoaders.Add(assetLoader);
                        assetLoader.OnCompleteLoading += asset =>
                        {
                            if (--dependencyCount == 0)
                            {
                                loadFunc();
                            }
                        };
                        AssetManager.Instance.RequestAssetLoad(assetLoader);
                    }
                }
            }
        }

        public void OnCompleteDownload(bool inValue, AssetBundle inBundle)
        {
            AssetObject = inBundle.LoadAsset(AssetName);

            _dependencyMainHook?.Invoke();

            IsDone = true;

            OnDone?.Invoke();
        }

        public void UnloadBundle()
        {
            _bundleUnloadCallback?.Invoke();
        }
    }

    public class BulkAssetBundleLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public bool DeferredRelease = false;

        public static Dictionary<string, Queue<BulkAssetBundleLoader>> PendingBundleRequest = new Dictionary<string, Queue<BulkAssetBundleLoader>>();
        public static void OnBundleDownloaded(bool inValue, UnityAssetLoader.AsyncBundleWebRequest request)
        {
            if (!inValue)
            {
                return;
            }

            bool isReleaseDeferred = false;

            if (PendingBundleRequest.ContainsKey(request.BundleName))
            {
                var pendingRequest = PendingBundleRequest[request.BundleName];

                if (pendingRequest != null)
                {
                    while (pendingRequest.Count > 0)
                    {
                        var r = pendingRequest.Dequeue();
                        r.OnCompleteDownload(true, request.AssetBundle);
                        if (r.DeferredRelease)
                        {
                            r._bundleUnloadCallback = () =>
                            {
                                if (request.AssetBundle != null)
                                {
                                    request.AssetBundle.Unload(false);
                                    request.AssetBundle = null;
                                }
                            };
                            isReleaseDeferred = true;
                        }
                    }
                }

                PendingBundleRequest[request.BundleName].Clear();
                PendingBundleRequest.Remove(request.BundleName);
            }

            if (!isReleaseDeferred
                && request.AssetBundle != null)
            {
                request.AssetBundle.Unload(false);
                request.AssetBundle = null;
            }
        }

        public string[] AssetGUIDs { get; set; }
        public string[] AssetNames { get; set; }
        public string BundleName { get; set; }
        public UnityEngine.Object[] AssetObjects { get; set; }

        private Action _bundleUnloadCallback;
        private Action _dependencyMainHook;

        public BulkAssetBundleLoader(string bundleNames)
        {
            var bundleData = AssetManager.Instance.GetBundleInfoByName(bundleNames);
            if (bundleData != null)
            {
                AssetGUIDs = bundleData.guids;
                AssetNames = bundleData.assetNames;
                BundleName = bundleNames;
            }

            UseCustomFunction = true;
        }

        public static BulkAssetBundleLoader FromGUIDS(string bundleName)
        {
            BulkAssetBundleLoader loader = new BulkAssetBundleLoader(bundleName);
            return loader;
        }

        public override void Load()
        {

            var bundleData = AssetManager.Instance.GetBundleInfoByName(BundleName);

            if (bundleData == null)
            {
                IsDone = true;
                HasFailed = true;
                OnDone?.Invoke();

                return;
            }

            // if we find the bundle is in flight then we can just piggy back off its completion!
            if (PendingBundleRequest.ContainsKey(BundleName))
            {
                PendingBundleRequest[BundleName].Enqueue(this);
            }
            else
            {
                PendingBundleRequest[BundleName] = new Queue<BulkAssetBundleLoader>();
                PendingBundleRequest[BundleName].Enqueue(this);

                Action loadFunc = () =>
                {
                    var request = new UnityAssetLoader.AsyncBundleWebRequest();
                    request.URL = UnityAssetUtils.GetAssetURL(BundleName);
                    request.BundleHash = bundleData.hash;
                    request.OnCompleteDownload += OnBundleDownloaded;
                    request.BundleName = BundleName;

                    GameManager.Instance.StartCoroutine(request.DownloadRequest());
                };

                if (bundleData.dependencies == null
                        || bundleData.dependencies.Length == 0)
                {
                    loadFunc();
                }
                else
                {
                    List<BulkAssetBundleLoader> dependencyLoaders = new List<BulkAssetBundleLoader>();
                    int dependencyCount = bundleData.dependencies.Length;
                    _dependencyMainHook += () =>
                    {
                        for (int k = 0; k < dependencyLoaders.Count; k++)
                        {
                            dependencyLoaders[k].UnloadBundle();
                        }
                    };
                    for (int i = 0; i < bundleData.dependencies.Length; i++)
                    {
                        Debug.Log("depends " + bundleData.dependencies[i] + " " + bundleData.name);
                        AssetManifest.BundleInfo dependencyBundle = AssetManager.Instance.GetBundleInfoByName(bundleData.dependencies[i]);
                        Debug.Log("dependencyBundle " + dependencyBundle.name);

                        BulkAssetBundleLoader assetLoader = BulkAssetBundleLoader.FromGUIDS(bundleData.dependencies[i]);
                        assetLoader.DeferredRelease = true;
                        dependencyLoaders.Add(assetLoader);
                        assetLoader.OnCompleteLoading += (asset) =>
                        {
                            BulkAssetBundleLoader bulkAssetBundleLoader = asset as BulkAssetBundleLoader;
                            if (bulkAssetBundleLoader != null)
                                Debug.Log("onCompleteLoading " + bulkAssetBundleLoader.BundleName + " " + dependencyCount);

                            if (--dependencyCount == 0)
                            {
                                BulkAssetBundleLoader assetBundleLoader = asset as BulkAssetBundleLoader;
                                if (assetBundleLoader != null)
                                    Debug.Log("Loading all " + assetBundleLoader.BundleName);

                                loadFunc();
                            }
                        };
                        AssetManager.Instance.RequestAssetLoad(assetLoader);
                    }
                }
            }
        }

        public void OnCompleteDownload(bool inValue, AssetBundle inBundle)
        {
            AssetObjects = inBundle.LoadAllAssets();
            _dependencyMainHook?.Invoke();
            IsDone = true;
            OnDone?.Invoke();
        }

        public void UnloadBundle()
        {
            if (_bundleUnloadCallback != null)
            {
                _bundleUnloadCallback();
            }
        }
    }


    public class AsyncWebLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public string URL { get; set; }
        public byte[] WebData { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public AsyncWebLoader(string inURL)
        {
            URL = inURL;
            UseCustomFunction = true;

            Headers = new Dictionary<string, string>();
        }

        public override void Load()
        {
            var request = new Data.UnityAssetLoader.AsyncWebRequest(URL);
            request.OnCompleteDownload += OnCompleteDownload;
            request.Headers = Headers;

            GameManager.Instance.StartCoroutine(request.DownloadRequest());
        }

        public void OnCompleteDownload(bool inValue, UnityAssetLoader.AsyncWebRequest request)
        {
            // so we have failed in this case ? trying to download some object ? should we retry ? 
            if (!inValue)
            {
                HasFailed = true;
                IsDone = true;
                OnDone?.Invoke();

                return;
            }

            WebData = request.GetData();
            IsDone = true;
            OnDone?.Invoke();
        }
    }

    public class AsyncPollingWebLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public string URL { get; set; }
        public byte[] WebData { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        private readonly UnityAssetLoader.AsyncWebRequest _request;

        public AsyncPollingWebLoader(string inURL)
        {
            URL = inURL;
            UseCustomFunction = true;

            Headers = new Dictionary<string, string>();

            _request = new LibGameClient.Data.UnityAssetLoader.AsyncWebRequest(URL);
            _request.OnCompleteDownload += OnCompleteDownload;
        }

        public override void Load()
        {
            IsDone = false;

            _request.Reset();
            _request.Headers = Headers;
            GameManager.Instance.StartCoroutine(_request.DownloadRequest());
        }

        public void OnCompleteDownload(bool inValue, UnityAssetLoader.AsyncWebRequest request)
        {
            // so we have failed in this case ? trying to download some object ? should we retry ? 
            if (!inValue)
            {
                HasFailed = true;
                IsDone = true;

                OnDone?.Invoke();

                return;
            }

            WebData = request.GetData();
            IsDone = true;

            OnDone?.Invoke();
        }
    }

    public class AsyncPostWebLoader : LibCommon.Manager.Assets.AssetLoadRequest
    {
        public string URL { get; set; }
        public byte[] WebData { get; set; }
        public byte[] PostData { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public AsyncPostWebLoader(string inURL)
        {
            URL = inURL;
            UseCustomFunction = true;

            Headers = new Dictionary<string, string>();
        }

        public override void Load()
        {
            var request = new LibGameClient.Data.UnityAssetLoader.AsyncPostWebRequest(URL);
            request.OnCompleteDownload += OnCompleteDownload;

            foreach (var item in Headers)
            {
                request.Request.Headers[item.Key] = item.Value;
            }

            request.PostData = PostData;
            GameManager.Instance.StartCoroutine(request.DownloadRequest());
        }

        public void OnCompleteDownload(bool inValue, UnityAssetLoader.AsyncPostWebRequest request)
        {
            // so we have failed in this case ? trying to download some object ? should we retry ? 
            if (!inValue)
            {
                HasFailed = true;
                IsDone = true;

                OnDone?.Invoke();

                return;
            }

            WebData = request.GetData();
            IsDone = true;

            OnDone?.Invoke();
        }
    }

    public class UnityAssetLoader
    {
        public class AsyncBundleWebRequest
        {
            private readonly object _syncObj = new object();

            public string URL = "";
            public string BundleHash = "";
            public string BundleName = "";

            public Action<bool, AsyncBundleWebRequest> OnCompleteDownload;
            public AssetBundle AssetBundle { get; set; }

            // we need to call this manually! 
            public IEnumerator DownloadRequest()
            {
                uint hashValue = 0;

                uint.TryParse(BundleHash, out hashValue);

                var bundle = WWW.LoadFromCacheOrDownload(URL, 0, hashValue);

                yield return bundle;

                if (!string.IsNullOrEmpty(bundle.error))
                {
                    // Do we requeue this ? for now NO! 
                    OnCompleteDownload?.Invoke(false, this);
                }

                AssetBundle = bundle.assetBundle;

                OnCompleteDownload?.Invoke(true, this);
            }
        }

        public class AsyncWebRequest
        {
            private readonly object _syncObj = new object();

            public string URL;

            public Action<bool, AsyncWebRequest> OnCompleteDownload;
            private bool _isDone = false;
            private System.IO.Stream _dataStream;
            private bool _wasSuccessful = true;
            public Dictionary<string, string> Headers { get; set; }

            public AsyncWebRequest(string inURL)
            {
                URL = inURL;
            }

            // we need to call this manually! 
            public IEnumerator DownloadRequest()
            {
                Action completionTask = () =>
                {
                    var request = WebRequest.Create(URL);
                    request.Timeout = 30000;
                    request.Method = WebRequestMethods.Http.Get;

                    foreach (KeyValuePair<string, string> item in Headers)
                    {
                        request.Headers[item.Key] = item.Value;
                    }

                    try
                    {
                        var response = request.GetResponse();

                        lock (_syncObj)
                        {
                            _isDone = true;
                            _dataStream = response.GetResponseStream();
                        }

                    }
                    catch (WebException)
                    {
                        // if we got here we have failed ? 
                        _isDone = true;
                        _wasSuccessful = false;

                        return;
                    }
                };
                ThreadPool.QueueUserWorkItem(_ => completionTask());

                while (!_isDone)
                    yield return null;

                OnCompleteDownload?.Invoke(_wasSuccessful, this);
            }

            public byte[] GetData()
            {
                return _dataStream.ReadAllBytes();
            }

            public void Reset()
            {
                _isDone = false;
            }
        }

        public class AsyncPostWebRequest
        {
            private readonly object _syncObj = new object();

            public string URL = "";

            public Action<bool, AsyncPostWebRequest> OnCompleteDownload;
            private bool _isDone = false;
            private System.IO.Stream _dataStream;
            private bool _wasSuccessful = true;
            public WebRequest Request { get; set; }
            public byte[] PostData { get; set; }

            public AsyncPostWebRequest(string inURL)
            {
                URL = inURL;
                Request = WebRequest.Create(URL);
            }

            // we need to call this manually! 
            public IEnumerator DownloadRequest()
            {
                Action completionTask = () =>
                {
                    Request.Timeout = 30000;
                    Request.Method = WebRequestMethods.Http.Post;
                    Request.ContentType = "text/json";
                    Request.ContentLength = PostData.Length;

                    try
                    {
                        using (var ss = Request.GetRequestStream())
                        {
                            ss.Write(PostData, 0, PostData.Length);
                            ss.Close();
                        }

                        var response = Request.GetResponse();

                        lock (_syncObj)
                        {
                            _isDone = true;
                            _dataStream = response.GetResponseStream();
                        }
                    }
                    catch (WebException)
                    {
                        // if we got here we have failed ? 
                        _isDone = true;
                        _wasSuccessful = false;

                        return;
                    }
                };
                ThreadPool.QueueUserWorkItem(_ => completionTask());

                while (!_isDone)
                    yield return null;

                OnCompleteDownload?.Invoke(_wasSuccessful, this);
            }

            public byte[] GetData()
            {
                return _dataStream.ReadAllBytes();
            }
        }
    }
}
