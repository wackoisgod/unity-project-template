using System;
using LibCommon.Network;
using LibGameClient.Manager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using Object = UnityEngine.Object;

namespace LibGameClient.Network
{
    public class GameMessage : MessageBase
    {
        public byte[] content;

        public GameMessage(byte[] inValue)
        {
            content = inValue;
        }

        public GameMessage()
        {
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteBytesFull(content);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            content = reader.ReadBytesAndSize();
        }
    }

    public class UnityMatchmaking
    {
        public static UnityMatchmaking Instance { get; set; }

        public NetworkManager MasterServerConnection;
        private GameObject _masterGameObject;

        public NetworkID LastGameId;

        public void Init()
        {
            _masterGameObject = new GameObject("_internalMatchmaker");
            Object.DontDestroyOnLoad(_masterGameObject);

            if (MasterServerConnection == null)
            {
                Debug.Log("CloudId " + Application.cloudProjectId);
                Debug.Log("Device ID " + SystemInfo.deviceUniqueIdentifier);

                MasterServerConnection = _masterGameObject.AddComponent<NetworkManager>();
                MasterServerConnection.StartMatchMaker();
                MasterServerConnection.matchMaker.SetProgramAppID((AppID) 222651);
            }
        }
    }

    public class UnityNetworkObject : MonoBehaviour
    {
        private const ushort PacketSize = 15000;

        private NetworkClient _clientConnection;

        public bool IsServer;

        public Action<byte[]> OnServerPacket;
        public Action<byte[], string> OnClientPacket;

        public Action OnConnected;
        public Action OnFailedConnection;
        public Action OnDisconnect;
        public Action<string> OnPlayerConnect;
        public Action<string> OnPlayerDisconnect;

        public void Init(bool server)
        {
            IsServer = server;

            if (IsServer) return;

            _clientConnection = new NetworkClient();

            ConnectionConfig config = new ConnectionConfig();
            int myReiliableChannelId = config.AddChannel(QosType.ReliableFragmented);
            int myUnreliableChannelId = config.AddChannel(QosType.UnreliableFragmented);

            _clientConnection.Configure(config, 3);

            _clientConnection.RegisterHandler(100, OnServerMessage);
            _clientConnection.RegisterHandler(MsgType.Connect, OnServerConnect);
            _clientConnection.RegisterHandler(MsgType.Disconnect, OnServerDisconnect);
        }

        void OnConnectedToServer()
        {
            OnConnected?.Invoke();
        }

        void OnFailedToConnect(NetworkConnectionError error)
        {
            OnFailedConnection?.Invoke();
        }

        void OnDisconnectFromServer()
        {
            OnDisconnect?.Invoke();
        }

        public void LaunchListenServer()
        {
            if (UnityMatchmaking.Instance.MasterServerConnection != null)
            {
                Debug.Log("Creating match");

                CreateMatchRequest create = new CreateMatchRequest();
                create.name = GameManager.Instance.PlayerName;
                create.size = 3;
                create.advertise = true;
                create.password = "";

                UnityMatchmaking.Instance.MasterServerConnection.matchMaker.CreateMatch(create, OnMatchCreate);
            }
        }

        public void OnMatchCreate(CreateMatchResponse matchResponse)
        {
            if (matchResponse.success)
            {
                Debug.Log("Create match succeeded");

                Debug.Log(matchResponse.networkId);
                Debug.Log(Utility.GetSourceID().ToString());

                ConnectionConfig config = new ConnectionConfig();
                int myReiliableChannelId = config.AddChannel(QosType.ReliableFragmented);
                int myUnreliableChannelId = config.AddChannel(QosType.UnreliableFragmented);

                NetworkServer.Configure(config, 3);

                NetworkServer.RegisterHandler(MsgType.Connect, OnPlayerConnected);
                NetworkServer.RegisterHandler(100, OnClientMessage);
                NetworkServer.Listen(new MatchInfo(matchResponse), 25000);

                Utility.SetAccessTokenForNetwork(matchResponse.networkId,
                    new NetworkAccessToken(matchResponse.accessTokenString));

                UnityMatchmaking.Instance.MasterServerConnection.matchMaker.JoinMatch(matchResponse.networkId, "",
                    OnServerMatchJoined);
                UnityMatchmaking.Instance.LastGameId = matchResponse.networkId;
            }
            else
            {
                Debug.LogError("Create match failed");
            }
        }

        public void OnServerMatchJoined(JoinMatchResponse matchJoin)
        {
            if (matchJoin.success)
            {
                _clientConnection = ClientScene.ConnectLocalServer();
            }
            else
            {
                Debug.LogError("Join match failed");
            }
        }

        public void OnMatchDestroy(BasicResponse matchResponse)
        {
            if (matchResponse.success)
            {
                Debug.Log("YAY");
            }
        }

        public void OnPlayerConnected(NetworkMessage msg)
        {
            Debug.Log("Player Connected!");
        }

        public void OnServerConnect(NetworkMessage msg)
        {
            Debug.Log("OnServerConnect");

            OnConnectedToServer();
        }

        public void OnServerDisconnect(NetworkMessage msg)
        {
            Debug.Log("OnServerDisconnect");

            OnDisconnectFromServer();
        }

        public void OnClientMessage(NetworkMessage msg)
        {
            var gm = msg.ReadMessage<GameMessage>();

            OnClientPacket?.Invoke(gm.content, msg.conn.connectionId.ToString());
        }

        public void OnServerMessage(NetworkMessage msg)
        {
            var gm = msg.ReadMessage<GameMessage>();

            OnServerPacket?.Invoke(gm.content);
        }

        public void ConnectToServer(string ip)
        {
            _clientConnection?.Connect(ip, 25000);
        }

        public void ConnectToServer(MatchInfo ip)
        {
            _clientConnection?.Connect(ip);
        }

        // called when the server has been initialized
        void OnServerInitialized()
        {
            Debug.Log("Server initialized");
        }

        public void BroadcastAll(byte[] inValue)
        {
            if (NetworkServer.active)
            {
                var bb = new GameMessage(inValue);
                NetworkServer.SendToAll(100, bb);
            }
        }

        public void SendPacketToClient(string inGUID, byte[] inValue)
        {
            if (NetworkServer.active)
            {
                try
                {
                    int connId = int.Parse(inGUID);
                    {
                        var bb = new GameMessage(inValue);
                        NetworkServer.SendToClient(connId, 100, bb);
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("Failed to Find a Player");
                }
            }
        }

        public void SendToServer(byte[] inValue)
        {
            var bb = new GameMessage(inValue);

            if (_clientConnection != null)
                _clientConnection.Send(100, bb);
            else
            {
                OnClientPacket?.Invoke(inValue, "-1");
            }
        }

        public void Shutdown()
        {
            if (!IsServer)
            {
                if (_clientConnection != null)
                {
                    _clientConnection.Disconnect();
                    _clientConnection = null;
                }
            }
        }
    }

    public class UNetNetServerDriver : BaseNetDriver
    {
        private readonly UnityNetworkObject _internalNetwork;
        private readonly GameObject _networkGameObject;

        public UNetNetServerDriver()
        {
            _networkGameObject = new GameObject("_internalListenServer");
            _internalNetwork = _networkGameObject.AddComponent<UnityNetworkObject>();
            _internalNetwork.OnServerPacket += OnServerPacket;
            _internalNetwork.OnClientPacket += OnClientPacket;


            _internalNetwork.Init(true);

            // we launch the local server!
            _internalNetwork.LaunchListenServer();

            GameObject.DontDestroyOnLoad(_networkGameObject);
        }

        public override void SendCommandRemote(QueueType inQueue,
            LibCommon.Network.Types.GameMessage inEvent)
        {
            var msg = ThriftMessageSerialize.SerializeCompact(inEvent);

            if (inQueue == QueueType.Client)
            {
                if (String.IsNullOrEmpty(inEvent.NetworkId))
                {
                    //_internalNetwork.BroadcastAll(NetworkUtils.SerializeEvent(inEvent));

                    _internalNetwork.BroadcastAll(msg);

                    ClientQueue.Enqueue(inEvent);
                }
                else
                {
                    Debug.Log(inEvent.NetworkId);

                    if (inEvent.NetworkId == "-1")
                    {
                        ClientQueue.Enqueue(inEvent);
                    }
                    else
                    {
                        _internalNetwork.SendPacketToClient(inEvent.NetworkId, msg);
                    }
                }
            }
            else
            {
                _internalNetwork.SendToServer(msg);
            }
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

        public void OnServerPacket(byte[] inValue)
        {
        }

        public void OnClientPacket(byte[] inValue, string inID)
        {
            var msg = new LibCommon.Network.Types.GameMessage();
            ThriftMessageSerialize.DeSerializeCompact(msg, inValue);

            msg.NetworkId = inID;

            if (msg != null)
            {
                ServerQueue.Enqueue(msg);
            }
        }

        public override void SendCommandLocal(QueueType inQueue,
            LibCommon.Network.Types.GameMessage inEvent)
        {
            switch (inQueue)
            {
                case QueueType.Client:
                    ClientQueue.Enqueue(inEvent);
                    break;
                case QueueType.Server:
                    ServerQueue.Enqueue(inEvent);
                    break;
                default:
                    break;
            }
        }

        public override void Shutdown()
        {
            var d = new DestroyMatchRequest();
            d.networkId = 0;

            Debug.Log(Utility.GetAppID().ToString());
            Debug.Log(Utility.GetAccessTokenForNetwork(d.networkId).GetByteString());

            UnityMatchmaking.Instance.MasterServerConnection.matchMaker.DestroyMatch(d, OnMatchDestroy);
        }

        public void OnMatchDestroy(BasicResponse matchResponse)
        {
            if (matchResponse.success)
            {
                Debug.Log("YAY");
            }

            NetworkServer.DisconnectAll();
            NetworkServer.Shutdown();
            Object.DestroyImmediate(_networkGameObject);
        }
    }

    public class UNetNetClientDriver : BaseNetDriver
    {
        private readonly UnityNetworkObject _internalNetwork;
        private readonly GameObject _networkGameObject;

        public Action OnConnected;
        public Action OnFailedConnection;
        public Action OnDisconnected;

        public UNetNetClientDriver(string inIP)
        {
            _networkGameObject = new GameObject("_internalListenClient");
            _internalNetwork = _networkGameObject.AddComponent<UnityNetworkObject>();
            _internalNetwork.OnServerPacket += OnServerPacket;
            _internalNetwork.OnClientPacket += OnClientPacket;
            _internalNetwork.OnConnected += OnClientConnected;
            _internalNetwork.OnFailedConnection += OnFailedConnected;
            _internalNetwork.OnDisconnect += OnClientDisconnected;

            _internalNetwork.Init(false);

            // we launch the local server!
            _internalNetwork.ConnectToServer(inIP);

            GameObject.DontDestroyOnLoad(_networkGameObject);
        }

        public UNetNetClientDriver(MatchInfo inIP)
        {
            GameObject network = new GameObject("_internalListenClient");
            _internalNetwork = network.AddComponent<UnityNetworkObject>();
            _internalNetwork.OnServerPacket += OnServerPacket;
            _internalNetwork.OnClientPacket += OnClientPacket;
            _internalNetwork.OnConnected += OnClientConnected;
            _internalNetwork.OnFailedConnection += OnFailedConnected;
            _internalNetwork.OnDisconnect += OnClientDisconnected;

            _internalNetwork.Init(false);

            // we launch the local server!
            _internalNetwork.ConnectToServer(inIP);

            GameObject.DontDestroyOnLoad(network);
        }

        public void OnServerPacket(byte[] inValue)
        {
            var msg = new LibCommon.Network.Types.GameMessage();

            ThriftMessageSerialize.DeSerializeCompact(msg, inValue);
            ClientQueue.Enqueue(msg);
        }

        public void OnClientPacket(byte[] inValue, string inID)
        {
        }

        public void OnClientConnected()
        {
            OnConnected?.Invoke();
        }

        public void OnFailedConnected()
        {
            OnFailedConnection?.Invoke();
        }

        public void OnClientDisconnected()
        {
            OnDisconnected?.Invoke();
        }

        public override void SendCommandRemote(QueueType inQueue,
            LibCommon.Network.Types.GameMessage inEvent)
        {
            var msg = ThriftMessageSerialize.SerializeCompact(inEvent);

            if (inQueue == QueueType.Client)
            {
                ClientQueue.Enqueue(inEvent);
            }
            else
            {
                _internalNetwork.SendToServer(msg);
            }
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

        public override void SendCommandLocal(QueueType inQueue,
            LibCommon.Network.Types.GameMessage inEvent)
        {
            switch (inQueue)
            {
                case QueueType.Client:
                    ClientQueue.Enqueue(inEvent);
                    break;
                case QueueType.Server:
                    ServerQueue.Enqueue(inEvent);
                    break;
                default:
                    break;
            }
        }

        public override void Shutdown()
        {
            _internalNetwork.Shutdown();
            Object.DestroyImmediate(_networkGameObject);
        }
    }
}
