using System;
using DriverAssist.Cruise;
using DriverAssist.Localization;
using DV.Rain;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public class DriverAssistController
    {
        private const int CC_SPEED_STEP = 5;

        private LocoController loco;
        private CruiseControl cruiseControl;
        private readonly UnifiedSettings config;
        private float updateAccumulator;
        private bool loaded = false;
        private GameObject gameObject;
        private DriverAssistWindow window;
        private ShiftSystem shiftSystem;
        private LocoStatsSystem locoStatsSystem;

#pragma warning disable CS8618
        public DriverAssistController(UnifiedSettings config)
#pragma warning restore CS8618
        {
            this.config = config;
        }

        public void Init()
        {
            PluginLoggerSingleton.Instance.Info($"Driver Assist is loaded!");

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
                locoStatsSystem.OnUpdate();
                shiftSystem.OnUpdate();
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
            gameObject = new GameObject("DriverAssistWindow");
            window = gameObject.AddComponent<DriverAssistWindow>();
            window.Config = config;
            Loaded += window.OnLoad;
            Unloaded += window.OnUnload;

            PluginLoggerSingleton.Instance.Info($"OnLoad");
            PlayerManager.CarChanged += OnCarChanged;

            loco = new LocoController(Time.fixedDeltaTime);

            cruiseControl = new CruiseControl(loco, config, new UnityClock())
            {
                Accelerator = new PredictiveAcceleration(),
                Decelerator = new PredictiveDeceleration()
            };

            shiftSystem = new ShiftSystem(loco);
            locoStatsSystem = new LocoStatsSystem(loco, 0.5f, Time.fixedDeltaTime);

            updateAccumulator = 0;
            loaded = true;
            Loaded?.Invoke(this, null);
            window.LocoController = loco;
            window.CruiseControl = cruiseControl;
        }

        public event EventHandler Loaded = delegate { };
        public event EventHandler Unloaded = delegate { };

        public void Unload()
        {
            PlayerManager.CarChanged -= OnCarChanged;

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
                DVTrainCarWrapper train = new(enteredCar);
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
            foreach (int intkey in keys)
            {
                KeyCode key = (KeyCode)intkey;
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

        UnityLogger()
        {
            Prefix = "";
        }

        public void Info(string message)
        {
            Debug.Log($"{Prefix}{message}");
        }
    }


}
