using System;
using DriverAssist.Cruise;
using DriverAssist.Localization;
using DV.Rain;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public class DriverAssistController
    {
        private static int CC_SPEED_STEP = 5;

        private LocoController loco;
        private CruiseControl cruiseControl;
        private UnifiedSettings config;
        private Translation localization;
        private float updateAccumulator;
        private bool loaded = false;
        private GameObject gameObject;
        private DriverAssistWindow window;

        public DriverAssistController(UnifiedSettings config)
        {
            this.config = config;
        }

        public void Init()
        {
            PluginLoggerSingleton.Instance.Info($"Driver Assist is loaded!");

            TranslationManager.Init();
            localization = TranslationManager.Current;

            WorldStreamingInit.LoadingFinished += OnLoadingFinished;
            UnloadWatcher.UnloadRequested += OnUnloadRequested;


            // we might load it with f6 in script engine, so onload would not occur
            if (PlayerManager.PlayerTransform != null)
            {
                PluginLoggerSingleton.Instance.Info($"Player detected");
                CheckForPlayer();
            }
        }

        public void OnDestroy()
        {
            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
            UnloadWatcher.UnloadRequested -= OnUnloadRequested;
        }

        public void OnFixedUpdate()
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

        public void OnUpdate()
        {
            if (!loaded) return;

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
            if (IsKeyPressed(config.DumpPorts))
            {
                foreach (var port in loco.Ports)
                {
                    PluginLoggerSingleton.Instance.Info($"{port}");
                }
            }
        }

        internal void Load()
        {
            PluginLoggerSingleton.Instance.Info($"Creating game object");
            GameObject gameObject = new GameObject("DriverAssistWindow");
            window = gameObject.AddComponent<DriverAssistWindow>();
            window.config = config;
            Loaded += window.OnLoad;
            Unloaded += window.OnUnload;

            PluginLoggerSingleton.Instance.Info($"OnLoad");
            PlayerManager.CarChanged += OnCarChanged;

            loco = new LocoController(Time.fixedDeltaTime);

            cruiseControl = new CruiseControl(loco, config, new UnityClock());
            cruiseControl.Accelerator = new PredictiveAcceleration();
            cruiseControl.Decelerator = new PredictiveDeceleration();

            updateAccumulator = 0;
            loaded = true;
            Loaded?.Invoke(this, null);
            window.loco = loco;
            window.cruiseControl = cruiseControl;
        }

        public event EventHandler Loaded = delegate { };
        public event EventHandler Unloaded = delegate { };

        public void Unload()
        {
            PlayerManager.CarChanged -= OnCarChanged;

            // Terminal.Shell.Commands.Remove(CC_CMD);
            // Terminal.Shell.Variables.Remove(CC_CMD);
            // Debug.Log($"OnDestroy");
            cruiseControl = null;
            loaded = false;
            Unloaded?.Invoke(this, null);

            Loaded -= window.OnLoad;
            Unloaded -= window.OnUnload;
            UnityEngine.Object.Destroy(window);
            UnityEngine.Object.Destroy(gameObject);
        }
        public void LoadIfNotLoaded()
        {
            if (!loaded)
            {
                Load();
            }
        }

        public void ChangeCar(TrainCar enteredCar)
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

        public void CheckForPlayer()
        {
            LoadIfNotLoaded();
            ChangeCar(PlayerManager.Car);
        }

        public void OnLoadingFinished()
        {
            PluginLoggerSingleton.Instance.Info($"OnLoadingFinished");
            // LoadIfNotLoaded();
            ChangeCar(PlayerManager.Car);
        }

        public void OnUnloadRequested()
        {
            PluginLoggerSingleton.Instance.Info($"OnUnloadRequested");
            Unload();
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

        /// Player has entered or exited a car
        public void OnCarChanged(TrainCar enteredCar)
        {
            PluginLoggerSingleton.Instance.Info($"OnCarChanged");
            ChangeCar(enteredCar);
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

    class DriverAssistWindow : MonoBehaviour
    {
        public LocoController loco { get; internal set; }
        public CruiseControl cruiseControl { get; internal set; }
        public UnifiedSettings config { get; internal set; }

        private Rect windowRect;
        private bool loaded;
        private Translation localization;
        private float scale = 1.5f;

        void Awake()
        {
            PluginLoggerSingleton.Instance.Info("DriverAssistWindow::Awake");
            localization = TranslationManager.Current;
            windowRect = new Rect(20, 20, scale * 120, scale * 50);
        }

        void OnGUI()
        {
            // PluginLoggerSingleton.Instance.Info($"loco={loco} loaded={loaded}");
            if (loco == null) return;
            // PluginLoggerSingleton.Instance.Info($"IsLoco={loco.IsLoco}");
            if (!loaded) return;

            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            if (!loco.IsLoco) return;

            GUI.skin = DVGUI.skin;

            windowRect = GUILayout.Window(4444324, windowRect, GUIWindow, "stats");
        }

        private void GUIWindow(int id)
        {
            Window();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        protected void Window()
        {
            float Speed = loco.RelativeSpeedKmh;
            float Throttle = loco.Throttle;
            float Mass = loco.Mass;
            float powerkw = Mass * loco.RelativeAccelerationMs * loco.RelativeSpeedKmh / 3.6f / 1000;
            float Force = Mass * 9.8f / 2f;

            int labelwidth = (int)(scale * 100);
            int width = (int)(scale * 50);

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_SETPOINT, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{cruiseControl.DesiredSpeed}", GUILayout.Width(width));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_STATUS, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{cruiseControl.Status}", GUILayout.Width(scale * 150));
            GUILayout.EndHorizontal();

            if (config.ShowStats)
            {
                int mass = (int)(Mass / 1000);
                int locoMass = (int)(loco.LocoMass / 1000);
                int cargoMass = (int)(loco.CargoMass / 1000);

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

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_TORQUE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)loco.RelativeTorque}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_THROTTLE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)(loco.Throttle * 100)}%", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.Gear + 1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear Ratio", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.GearRatio}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Shifting", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.GearShiftInProgress}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Train Brake", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{loco.TrainBrake.ToString("F2")}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                float speed2 = 3f / 25f * (float)Math.PI * loco.WheelRadius * loco.Rpm / loco.GearRatio;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{speed2.ToString("N1")}", GUILayout.Width(width));
                GUILayout.EndHorizontal();
            }
        }

        void OnDestroy()
        {
            PluginLoggerSingleton.Instance.Info("DriverAssistWindow::OnDestroy");
        }

        public void OnLoad(object sender, EventArgs e)
        {
            PluginLoggerSingleton.Instance.Info("DriverAssistWindow::OnLoad");
            loaded = true;
        }

        public void OnUnload(object sender, EventArgs e)
        {
            PluginLoggerSingleton.Instance.Info("DriverAssistWindow::OnUnload");
            loaded = false;
        }
    }
}
