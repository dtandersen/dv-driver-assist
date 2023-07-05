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

        public void Info(string message)
        {
            logger.LogInfo(message);
        }
    }

    class UnityLogger : PluginLogger
    {
        public void Info(string message)
        {
            Debug.Log(message);
        }
    }
}
