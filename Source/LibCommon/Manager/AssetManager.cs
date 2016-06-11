using System;
using System.Collections.Generic;
using System.Threading;
using LibCommon.Assets;
using LibCommon.Data;
using LibCommon.Manager.Assets;

namespace LibCommon.Manager
{
  namespace Assets
  {
    public class AssetLoadRequest
    {
      public Action<AssetLoadRequest> OnCompleteLoading;
      public Action<AssetLoadRequest> OnFailedLoading;
      public Action OnDone;

      public bool Block { get; set; }
      public bool IsDone { get; protected set; }
      public bool HasFailed { get; protected set; }
      public bool UseCustomFunction { get; set; }

      public virtual void Load()
      {
      }

      public virtual void OnComplete()
      {
        OnCompleteLoading?.Invoke(this);
      }

      public virtual void OnFailure()
      {
        OnFailedLoading?.Invoke(this);
      }
    }
  }

  public class AssetManager : BaseManager
  {
    public static AssetManager Instance { get; private set; }

    private readonly Queue<AssetLoadRequest> _internalAssetQueue = new Queue<AssetLoadRequest>();
    private readonly Queue<AssetLoadRequest> _internalCompletedQueue = new Queue<AssetLoadRequest>();
    private readonly object _syncObj = new object();
    private const int MAXJOBS = 4;
    private int _runningTaskCount;

    private Dictionary<string, AssetManifest.BundleInfo> _bundleMapping;
    public Dictionary<string, AssetManifest.BundleInfo> BundleMapping => _bundleMapping;

    private Dictionary<string, AssetManifest.BundleInfo> _assetMapping;
    public Dictionary<string, AssetManifest.BundleInfo> AssetMapping => _assetMapping;

    public int AssetVersion { get; private set; }

    public override void Init()
    {
      if (Instance == null)
        Instance = this;

      _internalAssetQueue.Clear();
      _assetMapping = new Dictionary<string, AssetManifest.BundleInfo>();
      _bundleMapping = new Dictionary<string, AssetManifest.BundleInfo>();
    }

    public bool RequestAssetLoad(AssetLoadRequest inReuqest)
    {
      // should there be a max here ? 
      if (_internalAssetQueue.Count >= 100)
      {
        // we went over the max amount of assets that can be loaded or requested ? 
        return false;
      }

      lock (_syncObj)
      {
        _internalAssetQueue.Enqueue(inReuqest);
      }

      PumpAssetLoading();

      return true;
    }

    private void PumpAssetLoading()
    {
      lock (_syncObj)
      {
        if (_internalAssetQueue.Count > 0)
        {
          var task = _internalAssetQueue.Dequeue();
          if (task.UseCustomFunction)
          {
            Action handler = null;
            handler = () =>
            {
              task.OnDone -= handler;

              OnTaskCompletedAdd(task);
            };

            task.OnDone += handler;

            task.Load();

            _runningTaskCount++;

            return;
          }

          if (_runningTaskCount == MAXJOBS) return;

          if (task.Block)
          {
            task.Load();
            while (!task.IsDone)
            {
            }

            task.OnComplete();
          }
          else
          {
            QueueUserWorkItem(task);
          }
        }
      }
    }

    private void QueueUserWorkItem(AssetLoadRequest asset)
    {
      Action completionTask = () =>
      {
        asset.Load();

        while (!asset.IsDone)
        {
        }

        OnTaskCompletedAdd(asset);
      };

      _runningTaskCount++;

      ThreadPool.QueueUserWorkItem(_ => completionTask());
    }

    public void AddToCompleteQueue(AssetLoadRequest asset)
    {
      lock (_syncObj)
      {
        _internalCompletedQueue.Enqueue(asset);
      }
    }

    private void OnTaskCompletedAdd(AssetLoadRequest asset)
    {
      AddToCompleteQueue(asset);
      OnTaskCompleted();
    }

    private void OnTaskCompleted()
    {
      lock (_syncObj)
      {
        --_runningTaskCount;
      }
    }

    public override void Update(float time, float deltaTime)
    {
      base.Update(time, deltaTime);

      // we have completed lets pump the asset queue again!
      PumpAssetLoading();

      while (_internalCompletedQueue.Count != 0)
      {
        var cpTask = _internalCompletedQueue.Dequeue();
        if (cpTask.HasFailed)
        {
          cpTask.OnFailure();
        }
        else
        {
          cpTask.OnComplete();
        }
      }
    }

    public void LoadManifestFile(byte[] inData)
    {
      var manifestfile = Serializer.Deserialize<AssetManifest>(inData);
      if (manifestfile != null)
      {
        AssetVersion = manifestfile.version;

        foreach (AssetManifest.BundleInfo data in manifestfile.bundles)
        {
          for (int i = 0; i < data.guids.Length; i++)
          {
            if (!_assetMapping.ContainsKey(data.guids[i]))
              _assetMapping.Add(data.guids[i], data);
          }
          BundleMapping.Add(data.name, data);
        }
      }
    }

    public AssetManifest.BundleInfo GetBundleInfoByGUID(string inAssetGUID)
    {
      if (_assetMapping.Count == 0)
      {
        return null;
      }

      AssetManifest.BundleInfo data;
      if (_assetMapping.TryGetValue(inAssetGUID, out data) == false)
      {
        return null;
      }

      return data;
    }

    public AssetManifest.BundleInfo GetBundleInfoByName(string inName)
    {
      AssetManifest.BundleInfo data;
      return _bundleMapping.TryGetValue(inName, out data) ? data : null;
    }
  }
}
