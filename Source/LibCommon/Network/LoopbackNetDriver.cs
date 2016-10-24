using LibCommon.Network.Types;

namespace LibCommon.Network
{
  public class LoopbackNetDriver : BaseNetDriver
  {
    private readonly object _lockObject = new object();

    public override void SendCommandRemote(QueueType inQueue, GameMessage inMsg)
    {
      lock (_lockObject)
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
    }

    public override void SendCommandLocal(QueueType inQueue, GameMessage inMsg)
    {
      lock (_lockObject)
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
    }

    public override GameMessage PopCommand(QueueType inQueue)
    {
      lock (_lockObject)
      {
        GameMessage ee = null;
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
