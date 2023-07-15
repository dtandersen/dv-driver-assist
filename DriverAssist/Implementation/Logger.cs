using BepInEx.Logging;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class BepInExLogger : PluginLogger
    {
        private readonly ManualLogSource logger;

        public BepInExLogger(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public string Prefix { get; set; }

        public void Info(string message)
        {
            logger.LogInfo($"{Prefix}{message}");
        }
    }

    class UnityLogger : PluginLogger
    {
        public string Prefix { get; set; }

        public void Info(string message)
        {
            Debug.Log($"{Prefix}{message}");
        }
    }
}
