using UnityEngine;

namespace DriverAssist.Implementation
{
    class UnityLogger : Logger
    {
        private readonly string prefix;

        UnityLogger()
        {
            prefix = "";
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log($"{prefix}{message}");
        }

        public void Warn(string message)
        {
            UnityEngine.Debug.Log($"{prefix}{message}");
        }

        public void Debug(string message)
        {
            UnityEngine.Debug.Log($"{prefix}{message}");
        }
    }
}
