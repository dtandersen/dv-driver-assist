using BepInEx;
using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist.Implementation
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DriverAssistBepInExPlugin : BaseUnityPlugin
    {
        private UnityPresenter presenter;

        private void Awake()
        {
            PluginLoggerSingleton.Instance = new BepInExLogger(Logger);
            PluginLoggerSingleton.Instance.Prefix = "--------------------------------------------------> ";

            BepInExDriverAssistSettings config = new BepInExDriverAssistSettings(Config);

            presenter = new UnityPresenter(this, config);
            presenter.Init();
        }

        private void OnDestroy()
        {
            PluginLoggerSingleton.Instance.Info($"OnDestroy");
            presenter.Unload();
            presenter.OnDestroy();
        }

        private void Update()
        {
            presenter.OnUpdate();
        }

        private void FixedUpdate()
        {
            presenter.OnFixedUpdate();
        }

        private void OnGUI()
        {
            presenter.OnGui();
        }
    }

    public interface DriverAssistSettings
    {
        int[] AccelerateKeys { get; }
        int[] DecelerateKeys { get; }
        int[] ToggleKeys { get; }
        int[] Upshift { get; }
        int[] Downshift { get; }
        int[] DumpPorts { get; }
        bool ShowStats { get; }
    }

    public class UnityClock : Clock
    {
        public float Time2 { get { return Time.realtimeSinceStartup; } }
    }
}
