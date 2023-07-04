using UnityEngine;

namespace CruiseControlPlugin
{
    class UnityLogger : PluginLogger
    {
        public void Info(string message)
        {
            Debug.Log("zzzzzzzzz" + message);
        }
    }
}
