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

        private LocoController? loco;
        private CruiseControl? cruiseControl;
        private readonly UnifiedSettings config;
        private float updateAccumulator;
        private bool loaded = false;
        private GameObject? gameObject;
        private DriverAssistWindow? window;
        private ShiftSystem? shiftSystem;
        private LocoStatsSystem? locoStatsSystem;
        private readonly Logger logger = LogFactory.GetLogger(typeof(DriverAssistController));

        public DriverAssistController(UnifiedSettings config)
#pragma warning disable CS8618
#pragma warning restore CS8618
        {
            this.config = config;
        }

        public void Init()
        {
            logger.Info($"Init");

            WorldStreamingInit.LoadingFinished += OnLoadingFinished;
            UnloadWatcher.UnloadRequested += OnUnloadRequested;

            // we might already be in a loco (reloaded mod)
            if (PlayerManager.PlayerTransform != null)
            {
                logger.Info($"Player detected");
                CheckForPlayer();
            }
        }

        public void OnDestroy()
        {
            logger.Info("OnDestroy");

            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
            UnloadWatcher.UnloadRequested -= OnUnloadRequested;
        }

        public void OnFixedUpdate()
        {
            if (!loaded) return;

            if (loco?.IsLoco ?? false)
            {
                locoStatsSystem?.OnUpdate();
                shiftSystem?.OnUpdate();
                loco.UpdateStats(Time.fixedDeltaTime);
                updateAccumulator += Time.fixedDeltaTime;
                if (updateAccumulator > config.UpdateInterval)
                {
                    cruiseControl?.Tick();
                    updateAccumulator = 0;
                }
            }
        }

        public void OnUpdate()
        {
            if (!loaded) return;

            if (cruiseControl != null)
            {
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
            }

            if (loco != null)
            {
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
                        logger.Info($"{port}");
                    }
                }
            }
        }

        internal void Load()
        {
            loaded = true;
            logger.Info("Load");

            gameObject = new GameObject("DriverAssistWindow");
            window = gameObject.AddComponent<DriverAssistWindow>();
            window.Config = config;
            Loaded += window.OnLoad;
            Unloaded += window.OnUnload;

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
            Loaded?.Invoke(this, null);
            window.LocoController = loco;
            window.CruiseControl = cruiseControl;
        }

        public event EventHandler Loaded = delegate { };
        public event EventHandler Unloaded = delegate { };

        public void Unload()
        {
            logger.Info("Unload");
            if (!loaded)
            {
                logger.Warn("Tried to unload DriverAssist when it is not loaded");
                return;
            }

            PlayerManager.CarChanged -= OnCarChanged;

            loaded = false;
            Unloaded?.Invoke(this, null);

            if (window != null)
            {
                Loaded -= window.OnLoad;
                Unloaded -= window.OnUnload;
            }

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

        public void ChangeCar(TrainCar? trainCar)
        {
            logger.Info($"ChangeCar {trainCar?.carType.ToString() ?? "null"}");

            LoadIfNotLoaded();
            if (trainCar == null || !trainCar.IsLoco)
            {
                loco?.UpdateLocomotive(NullTrainCarWrapper.Instance);
                logger.Info($"Exited train car");
                if (cruiseControl != null)
                {
                    cruiseControl.Enabled = false;
                }
            }
            else
            {
                DVTrainCarWrapper train = new(trainCar);
                logger.Info($"Entered train car {trainCar?.carType.ToString() ?? "null"}");
                loco?.UpdateLocomotive(train);
            }
        }

        public void CheckForPlayer()
        {
            LoadIfNotLoaded();
            ChangeCar(PlayerManager.Car);
        }

        public void OnLoadingFinished()
        {
            logger.Info($"OnLoadingFinished");
            ChangeCar(PlayerManager.Car);
        }

        public void OnUnloadRequested()
        {
            logger.Info($"OnUnloadRequested");
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
        public void OnCarChanged(TrainCar? enteredCar)
        {
            logger.Info($"OnCarChanged {enteredCar?.carType.ToString() ?? "null"}");
            ChangeCar(enteredCar);
        }
    }

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
