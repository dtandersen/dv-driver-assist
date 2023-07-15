namespace DriverAssist
{
    interface PluginLogger
    {
        string Prefix { get; set; }

        public void Info(string message);
    }

    class PluginLoggerSingleton
    {
        public static PluginLogger Instance = new NullLogger();
    }

    internal class NullLogger : PluginLogger
    {
        public string Prefix { get; set; }

        public void Info(string message) { }
    }
}
