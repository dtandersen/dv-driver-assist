using System;

namespace DriverAssist
{
#pragma warning disable IDE1006
    public interface PluginLogger
#pragma warning restore IDE1006
    {
        string Prefix { get; set; }

        public void Info(string message);
    }

    public class PluginLoggerSingleton
    {
        [ThreadStatic]
        public static PluginLogger Instance = new NullLogger();
    }

    public class NullLogger : PluginLogger
    {
        public string Prefix { get; set; }

        public void Info(string message) { }

        public NullLogger()
        {
            Prefix = "";
        }
    }
}
