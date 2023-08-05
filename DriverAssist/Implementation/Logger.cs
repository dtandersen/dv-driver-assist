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
            Debug.Log($"{prefix}{message}");
        }

        public void Warn(string message)
        {
            Debug.Log($"{prefix}{message}");
        }
    }
}
