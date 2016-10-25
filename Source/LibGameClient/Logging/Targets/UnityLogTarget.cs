using System;
using LibCommon.Logging;

namespace LibGameClient.Logging.Targets
{
  public class UnityLogTarget : LogTarget
  {
    public UnityLogTarget(Logger.Level minLevel, Logger.Level maxLevel, bool includeTimeStamps)
    {
      MinimumLevel = minLevel;
      MaximumLevel = maxLevel;
      IncludeTimeStamps = includeTimeStamps;
    }

    public void Log(string message)
    {
      LogMessage(Logger.Level.Debug, "Default", message);
    }

    public override void LogMessage(Logger.Level level, string logger, string message)
    {
      if (true)
      {
        string timeStamp = IncludeTimeStamps ? "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] " : "";
        string logMessage = $"{timeStamp}[{level.ToString().PadLeft(5)}] [{logger}]: {message}";

        switch (level)
        {
          case Logger.Level.Debug:
          case Logger.Level.Trace:
          case Logger.Level.Info:
            UnityEngine.Debug.Log(logMessage);
            break;
          case Logger.Level.Warn:
            UnityEngine.Debug.LogWarning(logMessage);
            break;
          case Logger.Level.Error:
          case Logger.Level.Fatal:
            UnityEngine.Debug.LogError(logMessage);
            break;
        }
      }
    }
  }
}
