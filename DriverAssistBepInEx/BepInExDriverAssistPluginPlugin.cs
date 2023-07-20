using BepInEx;
using DriverAssistBepInEx;

namespace DriverAssist.Implementation
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class DriverAssistBepInExPlugin : BaseUnityPlugin
    {
        private UnityPresenter presenter;

        private void Awake()
        {
            PluginLoggerSingleton.Instance = new BepInExLogger(Logger);
            PluginLoggerSingleton.Instance.Prefix = "--------------------------------------------------> ";

            BepInExDriverAssistSettings config = new BepInExDriverAssistSettings(Config);

            presenter = new UnityPresenter(config);
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
}
