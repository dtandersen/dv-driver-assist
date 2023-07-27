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
        public static readonly ThreadLocal<CreateLogger> Factory;

        static LogFactory()
        {
            Factory = new ThreadLocal<CreateLogger>
            {
                Value = (scope) => new NullLogger()
            };
        }

        public static Logger GetLogger(string scope)
        {
            Logger logger = Factory.Value.Invoke(scope);
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
