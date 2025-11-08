using System;
using System.IO;
using log4net;
using log4net.Config;

namespace UnoLisServer.Common.Helpers
{
    public static class Logger
    {
        private static readonly ILog _log;

        static Logger()
        {
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            var configFile = new FileInfo("log4net.config");

            if (configFile.Exists)
                XmlConfigurator.Configure(logRepository, configFile);
            else
                BasicConfigurator.Configure(logRepository);

            _log = LogManager.GetLogger(typeof(Logger));
        }

        public static void Log(string message)
        {
            _log.Info(message);
            Console.WriteLine($"📘 {message}");
        }

        public static void Warn(string message)
        {
            _log.Warn(message);
            Console.WriteLine($"⚠️ {message}");
        }

        public static void Error(string message, Exception ex = null)
        {
            _log.Error(message, ex);
            Console.WriteLine($"❌ {message} {(ex != null ? ex.Message : "")}");
        }

        public static void Debug(string message)
        {
            _log.Debug(message);
        }
    }
}
