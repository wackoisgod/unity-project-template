using System;
using System.Collections;
using System.Text;
using LibCommon.Manager;
using LibCommon.Manager.Assets;
using LibCommon.Network;
using LibCommon.Network.Types;
using LibGameClient.Data;
using LibGameClient.Manager;
using UnityEngine;

namespace LibGameClient.Network
{
  public class HttpNetDriver : BaseNetDriver
  {
    private bool _running;
    private bool _requestingCurrently;

    private readonly AsyncPollingWebLoader _wwwPollingRequest;
    private readonly AsyncPostWebLoader _wwwSendMessageRequest;

    public Action OnConnected;

    private readonly string _coreGamserverUrl = GameManager.Instance.GAMESERVERURL;
    private readonly string _userId = "test";

    public HttpNetDriver(string inGameId)
    {
      string gameId = inGameId;

      // Example URL ? 
      _wwwPollingRequest =
          new AsyncPollingWebLoader(_coreGamserverUrl + "/match/" + gameId + "/player/" + _userId + "/inbox");
      _wwwSendMessageRequest = new AsyncPostWebLoader(_coreGamserverUrl + "/match/" + gameId + "/send");

      _wwwPollingRequest.OnCompleteLoading += OnServerPacker;
      _wwwPollingRequest.OnFailedLoading += OnServerPacker;

      _running = true;

      _coreGamserverUrl = GameManager.Instance.GAMESERVERURL;
    }

    public override LibCommon.Network.Types.GameMessage PopCommand(QueueType inQueue)
    {
      LibCommon.Network.Types.GameMessage ee = null;
      if (inQueue == QueueType.Client)
      {
        ee = ClientQueue.Dequeue();
      }
      else if (inQueue == QueueType.Server)
      {
        ee = ServerQueue.Dequeue();
      }

      return ee;
    }

    public override void SendCommandLocal(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg)
    {
      if (inQueue == QueueType.Client)
      {
        ClientQueue.Enqueue(inMsg);
      }
      else if (inQueue == QueueType.Server)
      {
        ServerQueue.Enqueue(inMsg);
      }
    }

    public override void SendCommandRemote(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg)
    {
      string msg = ThriftMessageSerialize.Serialize(inMsg);

      if (inQueue == QueueType.Client)
      {
        ClientQueue.Enqueue(inMsg);
      }
      else
      {
        _wwwSendMessageRequest.PostData = Encoding.UTF8.GetBytes(msg);
        AssetManager.Instance.RequestAssetLoad(_wwwSendMessageRequest);
      }
    }

    public override void Shutdown()
    {
      // TODO MAKE SURE WHEN WE DESTROY THIS WE NEED TO DO SOME CLEANUP 
      _running = false;
    }

    public IEnumerator OnPulling()
    {
      while (_running)
      {
        yield return new WaitForSeconds(0.25f);

        if (_requestingCurrently) continue;

        _requestingCurrently = true;
        AssetManager.Instance.RequestAssetLoad(_wwwPollingRequest);
      }
    }

    public void OnServerPacker(AssetLoadRequest inValue)
    {
      if (!_running) return;

      if (!inValue.HasFailed)
      {
        AsyncPollingWebLoader asyncPollingWebLoader = inValue as AsyncPollingWebLoader;
        byte[] jsonData = asyncPollingWebLoader?.WebData;

        if (jsonData != null)
        {
          string jsonString = Encoding.UTF8.GetString(jsonData);
          try
          {
            GameMessageList mm = new GameMessageList();
            ThriftMessageSerialize.DeSerialize(mm, jsonString);

            foreach (string item in mm.Messages)
            {
              LibCommon.Network.Types.GameMessage gm = new LibCommon.Network.Types.GameMessage();
              ThriftMessageSerialize.DeSerialize(gm, item);

              ClientQueue.Enqueue(gm);
            }
          }
          catch (Exception ex)
          {
            Debug.LogErrorFormat("Error Message: {0} {1}", ex.Message, jsonString);
          }
        }
      }

      _requestingCurrently = false;
    }

    public void OnGameJoin(bool joined)
    {
      if (!joined) return;

      OnConnected?.Invoke();
      GameManager.Instance.StartCoroutine(OnPulling());
    }
  }
}
