using System.Linq;

namespace LibCommon.Logging
{
    internal class LogRouter
    {
        public static void RouteMessage(Logger.Level level, string logger, string message)
        {
            if (!LogManager.IsEnabled)
                return;

            // if we don't have any active log-targets,
            if (LogManager.Targets.Count == 0)
                return; // just skip

            foreach (
                LogTarget target in
                    LogManager.Targets.Where(target => level >= target.MinimumLevel && level <= target.MaximumLevel))
            {
                target.LogMessage(level, logger, message);
            }
        }
    }
}
