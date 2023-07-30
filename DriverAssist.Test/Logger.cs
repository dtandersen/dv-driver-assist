using Xunit.Abstractions;

namespace DriverAssist.Test
{
    public class XunitLogger : Logger
    {
        private readonly ITestOutputHelper output;

        public static void Init(ITestOutputHelper output)
        {
            LogFactory.Factory.Value = (scope) => new XunitLogger(output);
        }

        public XunitLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Info(string message)
        {
            WriteLog(message, "INFO");
        }

        public void Warn(string message)
        {
            WriteLog(message, "WARN");
        }

        private void WriteLog(string message, string severity)
        {
            output.WriteLine($"{severity} {message}");
        }
    }
}
