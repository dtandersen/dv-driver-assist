namespace DriverAssist
{
    public interface PluginLogger
    {
        string Prefix { get; set; }

        public void Info(string message);
    }

    public class PluginLoggerSingleton
    {
        public static PluginLogger Instance = new NullLogger();
    }

    public class NullLogger : PluginLogger
    {
        public string Prefix { get; set; }

        public void Info(string message) { }
    }
}
