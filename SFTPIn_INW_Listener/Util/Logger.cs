using System;
using log4net;

namespace SFTPIn_INW_Listener
{
    internal sealed class Logger
    {
        private static readonly Logger _instance = new Logger();
        private readonly ILog _log;

        private Logger()
        {
            _log = LogManager.GetLogger("SFTPInINW");
        }

        public static Logger Instance { get { return _instance; } }

        public void Debug(string msg) { _log.Debug(msg); }
        public void Info(string msg) { _log.Info(msg); }
        public void Warn(string msg) { _log.Warn(msg); }
        public void Error(string msg) { _log.Error(msg); }
        public void Error(string msg, Exception ex) { _log.Error(msg, ex); }
    }
}