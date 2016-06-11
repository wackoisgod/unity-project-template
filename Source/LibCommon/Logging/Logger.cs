using System.Globalization;

namespace LibCommon.Logging
{
    public class Logger
    {
        public string Name { get; set; }

        public Logger(string inName)
        {
            Name = inName;
        }

        public enum Level
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        public void Trace(string message)
        {
            Log(Level.Trace, message, null);
        }

        public void Trace(string message, params object[] args)
        {
            Log(Level.Trace, message, args);
        }

        public void Error(string message)
        {
            Log(Level.Error, message, null);
        }

        public void Error(string message, params object[] args)
        {
            Log(Level.Error, message, args);
        }

        private void Log(Level level, string message, object[] args)
        {
            LogRouter.RouteMessage(level, Name,
                args == null ? message : string.Format(CultureInfo.InvariantCulture, message, args));
        }
    }
}
