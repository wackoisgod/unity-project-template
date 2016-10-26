using System;
using System.Threading;
using LibCommon.Commands;

namespace GameServer
{
  class Program
  {
    private static Thread _consoleReader;

    static void Main(string[] args)
    {
      _consoleReader = new Thread(ReadFromConsole) { IsBackground = true };
      _consoleReader.Start();

      CommandConsole.Instance.MessageLogged += LogCallback;
      CommandConsole.Instance.Init("");

      while (true)
      {

      }
    }

    private static void LogCallback(string obj)
    {
      Console.WriteLine(obj);
    }

    private static void ReadFromConsole()
    {
      string line;
      do
      {
        Console.Write("[Console]> ");
        line = Console.ReadLine();
        CommandConsole.Instance.Dispatch(line);
      }
      while (line != null);

      CommandConsole.Instance.Log("Console input terminated.");
    }
  }
}
