using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using CommandTerminal;
using DriverAssist.Algorithm;
using DriverAssist.Implementation;
using DV.HUD;
using DV.UI.LocoHUD;
using UnityEngine;

namespace DriverAssist
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DriverAssistPlugin : BaseUnityPlugin
    {
        private static int CC_SPEED_STEP = 5;
        private static string CC_CMD = "cc";

        private CruiseControl cruiseControl;
        private CruiseControlTarget target;
        private PlayerLocoController loco;
        private CruiseControlConfig config;
        private float updateAccumulator;

        private void Awake()
        {
            PluginLoggerSingleton.Instance = new BepInExLogger(Logger);
            target = new CruiseControlTarget();
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_GUID}) is loaded!");
            loco = new PlayerLocoController();
            config = new BepInExCruiseControlConfig(Config);
            cruiseControl = new CruiseControl(loco, config);
            cruiseControl.Accelerator = new DefaultAccelerationAlgo();
            cruiseControl.Decelerator = new DefaultDecelerationAlgo();
            RegisterCommands1();

            updateAccumulator = 0;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            {
                cruiseControl.Enabled = !cruiseControl.Enabled;
            }
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                cruiseControl.DesiredSpeed += CC_SPEED_STEP;
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                cruiseControl.DesiredSpeed -= CC_SPEED_STEP;
            }

            loco.UpdateAcceleration(Time.deltaTime);
            updateAccumulator += Time.deltaTime;
            if (updateAccumulator > config.UpdateInterval)
            {
                cruiseControl.Tick();
                updateAccumulator = 0;
            }
        }

        void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            float Speed = target.GetSpeed();
            float Throttle = target.GetThrottle();
            float Mass = target.GetMass();
            float Power = Mass * 9.8f / 2f * Speed / 3.6f;
            float Force = Mass * 9.8f / 2f;
            float Hoursepower = Power / 745.7f;
            float Torque = target.GetTorque();

            GUILayout.BeginArea(new Rect(0, 0, 300, 500));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Point");
            GUILayout.TextField($"{cruiseControl.DesiredSpeed}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Status");
            GUILayout.TextField($"{cruiseControl.Status}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Throttle");
            GUILayout.TextField($"{(int)(target.GetThrottle() * 100)}%");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed (km/h)");
            GUILayout.TextField($"{(int)target.GetSpeed()}");
            GUILayout.TextField($"{(int)(loco.Speed + loco.Acceleration * 10)}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Acceleration (m/s^2)");
            GUILayout.TextField($"{Math.Round(loco.Acceleration, 2)}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Force");
            GUILayout.TextField($"{Force}");
            GUILayout.EndHorizontal();

            float powerkw = Mass * loco.Acceleration * loco.Speed / 3.6f / 1000;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Power (kW)");
            GUILayout.TextField($"{(int)powerkw}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Horsepower");
            GUILayout.TextField($"{(int)(powerkw * 1.341f)}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Torque");
            GUILayout.TextField($"{Torque}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mass (t)");
            GUILayout.TextField($"{(int)(Mass / 1000)}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Temperature");
            GUILayout.TextField($"{(int)target.GetTemperature()}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Amps");
            GUILayout.TextField($"{(int)loco.Amps}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("RPM");
            GUILayout.TextField($"{(int)loco.Rpm}");
            GUILayout.EndHorizontal();

            // GUILayout.BeginHorizontal();
            // GUILayout.Label("Reverser");
            // GUILayout.TextField($"{loco.Reverser}");
            // GUILayout.EndHorizontal();

            GUILayout.EndArea();
            // GUILayout.Button("I am not inside an Area");
            // GUILayout.BeginArea(new Rect(Screen.width / 2, Screen.height / 2, 300, 300));
            // GUILayout.Button("I am completely inside an Area");
            // GUILayout.EndArea();
        }

        private void RegisterCommands1()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().StartCoroutine(RegisterCommands());
            }
        }

        private IEnumerator RegisterCommands()
        {
            yield return WaitFor.EndOfFrame;

            CommandInfo command = Terminal.Shell.AddCommand(CC_CMD, Cruise, 1, 1, "");
        }

        private static void Cruise(CommandArg[] args)
        {
            BaseUnityPlugin plugin = null;
            foreach (var info in Chainloader.PluginInfos)
            {
                var metadata = info.Value.Metadata;
                if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
                {
                    Debug.Log($"Found {PluginInfo.PLUGIN_GUID}");
                }
            }

            foreach (var info in Chainloader.PluginInfos)
            {
                var metadata = info.Value.Metadata;
                if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
                {
                    Debug.Log($"Found2 {PluginInfo.PLUGIN_GUID}");
                    plugin = info.Value.Instance;
                    break;
                }
            }

            Type type = plugin.GetType().BaseType;

            PropertyInfo prop = type.GetProperty("Speed");

            prop.SetValue(plugin, args[0].Float, null);
        }

        void OnDestroy()
        {
            Terminal.Shell.Commands.Remove(CC_CMD);
            Terminal.Shell.Variables.Remove(CC_CMD);
            Debug.Log($"OnDestroy");
            cruiseControl = null;
            tag = null;
        }

        private TrainCar GetLocomotive()
        {
            if (!PlayerManager.Car)
            {
                return null;
            }
            if (!PlayerManager.Car.IsLoco)
            {
                return null;
            }
            return PlayerManager.Car;
        }

        private void OnHUDChanged(HUDInterfacer.HUDChangeEvent obj)
        {
            Logger.LogInfo($"OnHUDChanged oldBase={obj.oldBase} oldControls={obj.oldControls} oldManager={obj.oldManager}");
            Logger.LogInfo($"OnHUDChanged newBase={obj.newBase} newControls={obj.newControls} newManager={obj.newManager}");
            if (!obj.newManager)
            {
                Logger.LogInfo("hud removed");
                return;
            }
            Logger.LogInfo("hud changed2");
            InteriorControlsManager manager = obj.newManager;
            HUDLocoControls locoControls = obj.newControls;

            LocoIndicatorReader indicatorReader = manager.indicatorReader;
            if (indicatorReader != null)
            {
                if ((bool)locoControls.basicControls.speedMeter)
                {
                    float speed = indicatorReader.speed.Value;
                    Logger.LogInfo(speed);
                }
            }
        }
    }
}
