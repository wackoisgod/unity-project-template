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

        private readonly string _coreGamserverURL = GameManager.Instance.GAMESERVERURL;
        private readonly string _gameID;
        private readonly string _userID;

        public HttpNetDriver(string inGameId)
        {
            _gameID = inGameId;

            // Exmaple URL ? 
            _wwwPollingRequest =
                new AsyncPollingWebLoader(_coreGamserverURL + "/match/" + _gameID + "/player/" + _userID + "/inbox");
            _wwwSendMessageRequest = new AsyncPostWebLoader(_coreGamserverURL + "/match/" + _gameID + "/send");

            _wwwPollingRequest.OnCompleteLoading += OnServerPacker;
            _wwwPollingRequest.OnFailedLoading += OnServerPacker;

            _running = true;

            _coreGamserverURL = GameManager.Instance.GAMESERVERURL;
        }

        public override LibCommon.Network.Types.GameMessage PopCommand(QueueType inQueue)
        {
            LibCommon.Network.Types.GameMessage ee = null;
            switch (inQueue)
            {
                case QueueType.Client:
                    ee = ClientQueue.Dequeue();
                    break;
                case QueueType.Server:
                    ee = ServerQueue.Dequeue();
                    break;
            }

            return ee;
        }

        public override void SendCommandLocal(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg)
        {
            switch (inQueue)
            {
                case QueueType.Client:
                    ClientQueue.Enqueue(inMsg);
                    break;
                case QueueType.Server:
                    ServerQueue.Enqueue(inMsg);
                    break;
                default:
                    break;
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

                if (!_requestingCurrently)
                {
                    _requestingCurrently = true;
                    AssetManager.Instance.RequestAssetLoad(_wwwPollingRequest);
                }
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
                        var mm = new GameMessageList();
                        ThriftMessageSerialize.DeSerialize(mm, jsonString);

                        foreach (string item in mm.Messages)
                        {
                            var gm = new LibCommon.Network.Types.GameMessage();
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
