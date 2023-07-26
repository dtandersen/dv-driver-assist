using System;
using System.Diagnostics;
using System.Threading;

namespace DriverAssist
{
    public delegate Logger CreateLogger(string scope);

    public interface Logger
    {
        public void Info(string message);
        public void Warn(string message);
    }

    public class LogFactory
    {
        // [ThreadStatic]
        public static ThreadLocal<CreateLogger> CreateLogger;

        static LogFactory()
        {
            UnityEngine.Debug.Log("LogFactory::LogFactory");
            CreateLogger = new ThreadLocal<CreateLogger>
            {
                Value = (scope) => new NullLogger()
            };
        }

        public static Logger GetLogger(string scope)
        {
            UnityEngine.Debug.Log("LogFactory::GetLogger scope");
            Logger logger = CreateLogger.Value.Invoke(scope);
            UnityEngine.Debug.Log($"LogFactory::GetLogger scope {logger.GetType().Name}");
            return logger;
        }
    }

    public class NullLogger : Logger
    {
        public void Info(string message) { }

        public void Warn(string message) { }
    }
}
