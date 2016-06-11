namespace LibCommon.Network
{
    public class LoopbackNetDriver : BaseNetDriver
    {
        private readonly object _lockObject = new object();

        public override void SendCommandRemote(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg)
        {
            lock (_lockObject)
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
        }

        public override void SendCommandLocal(QueueType inQueue, LibCommon.Network.Types.GameMessage inMsg)
        {
            lock (_lockObject)
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
        }

        public override LibCommon.Network.Types.GameMessage PopCommand(QueueType inQueue)
        {
            lock (_lockObject)
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
        }

        public override void Shutdown()
        {
        }
    }
}
