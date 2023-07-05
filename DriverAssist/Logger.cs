namespace DriverAssist
{
    interface PluginLogger
    {
        public void Info(string message);
    }

    class PluginLoggerSingleton
    {
        public static PluginLogger Instance = new NullLogger();
    }

    internal class NullLogger : PluginLogger
    {
        public void Info(string message) { }
    }
}
