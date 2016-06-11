using System.Collections.Generic;
using Thrift.Protocol;

namespace LibCommon.Network
{
    public abstract class BaseNetDriver
    {
        public enum QueueType
        {
            Client,
            Server
        }

        private static BaseNetDriver _instance;

        public static void CreateNetDriver(BaseNetDriver inDriver)
        {
            if (_instance == null)
                _instance = inDriver;
        }

        public static void Destroy()
        {
            if (_instance != null)
            {
                _instance.Shutdown();
                _instance = null;
            }
        }

        public static BaseNetDriver Instance => _instance;

        protected Queue<LibCommon.Network.Types.GameMessage> ClientQueue = new Queue<LibCommon.Network.Types.GameMessage>();
        protected Queue<LibCommon.Network.Types.GameMessage> ServerQueue = new Queue<LibCommon.Network.Types.GameMessage>();

        public abstract void SendCommandRemote(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg);
        public abstract void SendCommandLocal(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg);
        public abstract LibCommon.Network.Types.GameMessage PopCommand(QueueType inQueue);
        public abstract void Shutdown();

        public int QueueCount(QueueType inQueue)
        {
            int count = 0;
            switch (inQueue)
            {
                case QueueType.Client:
                    count = ClientQueue.Count;
                    break;
                case QueueType.Server:
                    count = ServerQueue.Count;
                    break;
            }

            return count;
        }
    }
}
