using System.Collections.Generic;
using LibCommon.Network.Types;

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

    protected Queue<GameMessage> ClientQueue = new Queue<GameMessage>();
    protected Queue<GameMessage> ServerQueue = new Queue<GameMessage>();

    public abstract void SendCommandRemote(QueueType inQueue, GameMessage inMsg);
    public abstract void SendCommandLocal(QueueType inQueue, GameMessage inMsg);
    public abstract GameMessage PopCommand(QueueType inQueue);
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
