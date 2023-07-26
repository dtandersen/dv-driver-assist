using System;
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
        public static ThreadLocal<CreateLogger> CreateLogger;

        static LogFactory()
        {
            CreateLogger = new ThreadLocal<CreateLogger>
            {
                Value = (scope) => new NullLogger()
            };
        }

        public static Logger GetLogger(string scope)
        {
            Logger logger = CreateLogger.Value.Invoke(scope);
            return logger;
        }

        public static Logger GetLogger(Type scope)
        {
            return GetLogger(scope.Name);
        }
    }

    public class NullLogger : Logger
    {
        public void Info(string message) { }

        public void Warn(string message) { }
    }
}
