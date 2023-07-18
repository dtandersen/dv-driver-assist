using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using CommandTerminal;
using DriverAssist.Cruise;
using DriverAssist.Implementation;
using DriverAssist.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DriverAssist
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DriverAssistPlugin : BaseUnityPlugin
    {
        private static int CC_SPEED_STEP = 5;
        // private static string CC_CMD = "cc";

        private CruiseControl cruiseControl;
        private LocoController loco;
        private BepInExDriverAssistSettings config;
        private float updateAccumulator;
        private Translation localization;
        // private TrainCarWrapper trainCarWrapper;
        bool loaded = false;

        private void Awake()
        {
            PluginLoggerSingleton.Instance = new BepInExLogger(Logger);
            PluginLoggerSingleton.Instance.Prefix = "--------------------------------------------------> ";

            TranslationManager.Init();
            localization = TranslationManager.Current;

            PluginLoggerSingleton.Instance.Info($"{PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_GUID}) is loaded!");
            // trainCarWrapper = NullTrainCarWrapper.Instance;
            // PlayerManager.PlayerChanged += OnPlayerChanged;
            // initialize event handlers
            WorldStreamingInit.LoadingFinished += OnLoadingFinished;
            UnloadWatcher.UnloadRequested += OnUnloadRequested;
            // SceneManager.activeSceneChanged += ChangedActiveScene;
            // RegisterCommands1();

            config = new BepInExDriverAssistSettings(Config);

            // we might load it with f6 in script engine, so onload would not occur
            if (PlayerManager.PlayerTransform != null)
            {
                PluginLoggerSingleton.Instance.Info($"Player detected");
                CheckForPlayer();
            }
        }

        // private void OnPlayerChanged()
        // {
        //     PluginLoggerSingleton.Instance.Info($"OnPlayerChanged");
        //     ChangePlayer();
        // }


        /// Game has finished loading
        private void OnLoadingFinished()
        {
            PluginLoggerSingleton.Instance.Info($"OnLoadingFinished");
            // LoadIfNotLoaded();
            ChangeCar(PlayerManager.Car);
        }

        /// Player has entered or exited a car
        void OnCarChanged(TrainCar enteredCar)
        {
            PluginLoggerSingleton.Instance.Info($"OnCarChanged");
            ChangeCar(enteredCar);
        }

        /// Game is unloading
        private void OnUnloadRequested()
        {
            PluginLoggerSingleton.Instance.Info($"OnUnloadRequested");
            Unload();
        }

        void OnDestroy()
        {
            PluginLoggerSingleton.Instance.Info($"OnDestroy");
            Unload();
            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
            UnloadWatcher.UnloadRequested -= OnUnloadRequested;
            // SceneManager.activeSceneChanged -= ChangedActiveScene;
            // PlayerManager.PlayerChanged -= PlayerCreated;
            // WorldStreamingInit.LoadingFinished += OnLoadingFinished;
        }

        private void LoadIfNotLoaded()
        {
            if (!loaded)
            {
                Load();
            }
        }

        public void Load()
        {
            PluginLoggerSingleton.Instance.Info($"OnLoad");
            PlayerManager.CarChanged += OnCarChanged;

            loco = new LocoController(Time.fixedDeltaTime);

            cruiseControl = new CruiseControl(loco, config, new UnityClock());
            cruiseControl.Accelerator = new PredictiveAcceleration();
            cruiseControl.Decelerator = new PredictiveDeceleration();

            updateAccumulator = 0;
            loaded = true;
        }

        void Unload()
        {
            PluginLoggerSingleton.Instance.Info($"OnUnload");
            PlayerManager.CarChanged -= OnCarChanged;

            // Terminal.Shell.Commands.Remove(CC_CMD);
            // Terminal.Shell.Variables.Remove(CC_CMD);
            // Debug.Log($"OnDestroy");
            cruiseControl = null;
            loaded = false;
        }

        private void CheckForPlayer()
        {
            LoadIfNotLoaded();
            ChangeCar(PlayerManager.Car);
        }

        // private void PlayerCreated()
        // {
        //     Load();
        //     PluginLoggerSingleton.Instance.Info($"PlayerCreated");
        //     // playerCreated = true;
        //     // loaded = true;
        // }

        // private void ChangedActiveScene(Scene current, Scene next)
        // {
        //     PluginLoggerSingleton.Instance.Info($"Scene is now {next.name}");
        //     // if (next.name == "game_w3")
        //     // {
        //     Load();
        //     // }


        //     // string currentName = current.name;

        //     // if (currentName == null)
        //     // {
        //     //     // Scene1 has been removed
        //     //     currentName = "Replaced";
        //     // }

        //     // Debug.Log("Scenes: " + currentName + ", " + next.name);
        // }

        void ChangeCar(TrainCar enteredCar)
        {
            LoadIfNotLoaded();
            if (enteredCar?.IsLoco ?? false)
            {
                DVTrainCarWrapper train = new DVTrainCarWrapper(enteredCar);
                PluginLoggerSingleton.Instance.Info($"Entered {train.LocoType}");
                loco.UpdateLocomotive(train);
            }
            else
            {
                loco.UpdateLocomotive(NullTrainCarWrapper.Instance);
                PluginLoggerSingleton.Instance.Info($"Exited");
                cruiseControl.Enabled = false;
            }
        }

        void Update()
        {
            // if (UnloadWatcher.isUnloading)
            // {
            //     // loaded = false;
            //     // UnityEngine.Object.Destroy(gameObject);
            //     // return;
            // }

            if (!loaded) return;
            // if (!loco.IsLoco)
            // {
            //     return;
            // }

            if (IsKeyPressed(config.ToggleKeys))
            {
                cruiseControl.Enabled = !cruiseControl.Enabled;
            }
            if (IsKeyPressed(config.AccelerateKeys))
            {
                cruiseControl.DesiredSpeed += CC_SPEED_STEP;
            }
            if (IsKeyPressed(config.DecelerateKeys))
            {
                cruiseControl.DesiredSpeed -= CC_SPEED_STEP;
            }
            if (IsKeyPressed(config.Upshift))
            {
                loco.Upshift();
            }
            if (IsKeyPressed(config.Downshift))
            {
                loco.Downshift();
            }
        }

        void FixedUpdate()
        {
            if (!loaded) return;

            if (loco.IsLoco)
            {
                loco.UpdateStats(Time.fixedDeltaTime);
                updateAccumulator += Time.fixedDeltaTime;
                if (updateAccumulator > config.UpdateInterval)
                {
                    cruiseControl.Tick();
                    updateAccumulator = 0;
                }
            }
        }

        void OnGUI()
        {
            if (!loaded) return;
            // if (PlayerManager.PlayerTransform == null || !loaded) return;

            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            if (!loco.IsLoco) return;

            float Speed = loco.RelativeSpeedKmh;
            float Throttle = loco.Throttle;
            float Mass = loco.Mass;
            float powerkw = Mass * loco.RelativeAccelerationMs * loco.RelativeSpeedKmh / 3.6f / 1000;
            float Force = Mass * 9.8f / 2f;

            GUILayout.BeginArea(new Rect(0, 0, 300, 500));
            int labelwidth = 100;
            int width = 50;

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_SETPOINT, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{cruiseControl.DesiredSpeed}", GUILayout.Width(width));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_STATUS, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{cruiseControl.Status}", GUILayout.Width(150));
            GUILayout.EndHorizontal();

            if (config.ShowStats)
            {
                // GUILayout.BeginHorizontal();
                // GUILayout.Label(localization.STAT_LOCOMOTIVE, GUILayout.Width(labelwidth));
                // GUILayout.TextField($"{loco.LocoType}", GUILayout.Width(100));
                // GUILayout.EndHorizontal();

                int mass = (int)(Mass / 1000);
                int locoMass = (int)(loco.LocoMass / 1000);
                int cargoMass = (int)(loco.CargoMass / 1000);

                // GUILayoutOption params[]=[GUILayout.Width(100)];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"", GUILayout.Width(labelwidth));
                GUILayout.Label(localization.TRAIN, GUILayout.Width(width));
                GUILayout.Label(localization.LOCO_ABBV, GUILayout.Width(width));
                GUILayout.Label(localization.CARGO, GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{localization.STAT_MASS}", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{mass}", GUILayout.Width(width));
                GUILayout.TextField($"{locoMass}", GUILayout.Width(width));
                GUILayout.TextField($"{cargoMass}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label("Traction Motors");
                // GUILayout.TextField($"{loco.TractionMotors}");
                // GUILayout.EndHorizontal();
                float predTime = 5f;

                GUILayout.BeginHorizontal();
                GUILayout.Label($"", GUILayout.Width(labelwidth));
                GUILayout.Label(localization.STAT_CURRENT, GUILayout.Width(width));
                GUILayout.Label($"{localization.STAT_CHANGE}/s", GUILayout.Width(width));
                GUILayout.Label($"{(int)predTime}s", GUILayout.Width(width));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_SPEED, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.RelativeSpeedKmh.ToString("N1")}", GUILayout.Width(width));
                GUILayout.TextField($"{loco.RelativeAccelerationMs.ToString("N3")}", GUILayout.Width(width));
                GUILayout.TextField($"{(loco.RelativeSpeedKmh + predTime * loco.RelativeAccelerationMs * 3.6f).ToString("N1")}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_TEMPERATURE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.Temperature.ToString("N1")}", GUILayout.Width(width));
                GUILayout.TextField($"{loco.TemperatureChange.ToString("N2")}", GUILayout.Width(width));
                GUILayout.TextField($"{(loco.Temperature + predTime * loco.TemperatureChange).ToString("N1")}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label(localization.STAT_TEMPERATURE_CHANGE);
                // GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_AMPS, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.Amps.ToString("N0")}", GUILayout.Width(width));
                GUILayout.TextField($"{loco.AmpsRoc.ToString("N1")}", GUILayout.Width(width));
                GUILayout.TextField($"{(loco.Amps + predTime * loco.AmpsRoc).ToString("N0")}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_RPM, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{Math.Round(loco.Rpm, 0)}", GUILayout.Width(width));
                GUILayout.TextField($"", GUILayout.Width(width));
                GUILayout.TextField($"", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label(localization.STAT_ACCELERATION);
                // GUILayout.TextField($"{Math.Round(loco.RelativeAccelerationMs, 2)}");
                // GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label("Force");
                // GUILayout.TextField($"{(int)Force}");
                // GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_TORQUE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)loco.RelativeTorque}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label(localization.STAT_POWER);
                // GUILayout.TextField($"{(int)powerkw}");
                // GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label(localization.STAT_HORSEPOWER);
                // GUILayout.TextField($"{(int)(powerkw * 1.341f)}");
                // GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_THROTTLE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)(loco.Throttle * 100)}%", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.Gear + 1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("GearA", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.GearboxA}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear B", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.GearboxB}", GUILayout.Width(width));
                GUILayout.EndHorizontal();


                // GUILayout.BeginHorizontal();
                // GUILayout.Label("ROC Amps");
                // GUILayout.TextField($"{(int)loco.AmpsRoc}");
                // GUILayout.EndHorizontal();

                // GUILayout.BeginHorizontal();
                // GUILayout.Label("Average Amps");
                // GUILayout.TextField($"{(int)loco.AverageAmps}");
                // GUILayout.EndHorizontal();

            }
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

        bool IsKeyPressed(int[] keys)
        {
            foreach (KeyCode key in keys)
            {
                if (!Input.GetKeyDown(key))
                    return false;
            }

            return true;
        }

        // private void RegisterCommands1()
        // {
        //     if (Application.isPlaying)
        //     {
        //         UnityEngine.Object.FindObjectOfType<Terminal>().StartCoroutine(RegisterCommands());
        //     }
        // }

        // private IEnumerator RegisterCommands()
        // {
        //     yield return WaitFor.EndOfFrame;

        //     CommandInfo command = Terminal.Shell.AddCommand(CC_CMD, Cruise, 1, 1, "");
        // }

        // private static void Cruise(CommandArg[] args)
        // {
        //     BaseUnityPlugin plugin = null;
        //     foreach (var info in Chainloader.PluginInfos)
        //     {
        //         var metadata = info.Value.Metadata;
        //         if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
        //         {
        //             Debug.Log($"Found {PluginInfo.PLUGIN_GUID}");
        //         }
        //     }

        //     foreach (var info in Chainloader.PluginInfos)
        //     {
        //         var metadata = info.Value.Metadata;
        //         if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
        //         {
        //             Debug.Log($"Found2 {PluginInfo.PLUGIN_GUID}");
        //             plugin = info.Value.Instance;
        //             break;
        //         }
        //     }

        //     Type type = plugin.GetType().BaseType;

        //     PropertyInfo prop = type.GetProperty("Speed");

        //     prop.SetValue(plugin, args[0].Float, null);
        // }

        private TrainCarWrapper GetLocomotive()
        {
            if (!PlayerManager.Car)
            {
                return NullTrainCarWrapper.Instance;
            }
            if (!PlayerManager.Car.IsLoco)
            {
                return NullTrainCarWrapper.Instance;
            }

            // if (!DVTrainCarWrapper.IsSameTrainCar2(trainCarWrapper, PlayerManager.Car))
            return new DVTrainCarWrapper(PlayerManager.Car);
            // else
            //     return trainCarWrapper;
        }

        // private TrainCar GetLocomotive()
        // {
        //     if (!PlayerManager.Car)
        //     {
        //         return null;
        //     }
        //     if (!PlayerManager.Car.IsLoco)
        //     {
        //         return null;
        //     }
        //     return PlayerManager.Car;
        // }

        // private void OnHUDChanged(HUDInterfacer.HUDChangeEvent obj)
        // {
        //     Logger.LogInfo($"OnHUDChanged oldBase={obj.oldBase} oldControls={obj.oldControls} oldManager={obj.oldManager}");
        //     Logger.LogInfo($"OnHUDChanged newBase={obj.newBase} newControls={obj.newControls} newManager={obj.newManager}");
        //     if (!obj.newManager)
        //     {
        //         Logger.LogInfo("hud removed");
        //         return;
        //     }
        //     Logger.LogInfo("hud changed2");
        //     InteriorControlsManager manager = obj.newManager;
        //     HUDLocoControls locoControls = obj.newControls;

        //     LocoIndicatorReader indicatorReader = manager.indicatorReader;
        //     if (indicatorReader != null)
        //     {
        //         if ((bool)locoControls.basicControls.speedMeter)
        //         {
        //             float speed = indicatorReader.speed.Value;
        //             Logger.LogInfo(speed);
        //         }
        //     }
        // }
    }

    public interface DriverAssistSettings
    {
        int[] AccelerateKeys { get; }
        int[] DecelerateKeys { get; }
        int[] ToggleKeys { get; }
        int[] Upshift { get; }
        int[] Downshift { get; }
        bool ShowStats { get; }
    }

    public class UnityClock : Clock
    {
        public float Time2 { get { return Time.realtimeSinceStartup; } }
    }
}
