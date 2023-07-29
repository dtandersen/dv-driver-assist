using System;
using DriverAssist.Cruise;
using DV.Utils;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public delegate void EnterLocoHandler(LocoController locoController);
    public delegate void NoArgsHandler();

    public class DriverAssistController
    {
        private const int CC_SPEED_STEP = 5;

        private LocoController? locoController;
        private CruiseControl? cruiseControl;
        private readonly UnifiedSettings config;
        private float updateAccumulator;
        private bool loaded = false;
        private GameObject? gameObject;
        private DriverAssistWindow? window;
        private readonly Logger logger = LogFactory.GetLogger(typeof(DriverAssistController));
        private SystemManager? systemManager;

        public event EventHandler Loaded = delegate { };
        public event EventHandler Unloaded = delegate { };
        public event EnterLocoHandler EnterLoco = delegate { };
        public event NoArgsHandler ExitLoco = delegate { };

        public PlayerCameraSwitcher? PlayerCameraSwitcher { get { return SingletonBehaviour<PlayerCameraSwitcher>.Instance; } }

        public DriverAssistController(UnifiedSettings config)
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
            if (!loaded)
            {
                logger.Warn("Called OnDestroy before Unload");
            }

            WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
            UnloadWatcher.UnloadRequested -= OnUnloadRequested;
        }

        public void OnFixedUpdate()
        {
            if (!loaded) return;

            if (locoController?.IsLoco ?? false)
            {
                systemManager?.Update();
                locoController.UpdateStats(Time.fixedDeltaTime);
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

            if (locoController != null)
            {
                if (IsKeyPressed(config.Upshift))
                {
                    locoController.Upshift();
                }
                if (IsKeyPressed(config.Downshift))
                {
                    locoController.Downshift();
                }
                if (IsKeyPressed(config.DumpPorts))
                {
                    foreach (var port in locoController.Ports)
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
            // Loaded += window.OnLoad;
            // Unloaded += window.OnUnload;
            EnterLoco += window.Show;
            ExitLoco += window.Hide;
            if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged += window.OnPhotoModeChanged;

            PlayerManager.CarChanged += OnCarChanged;


            locoController = new LocoController(Time.fixedDeltaTime);

            cruiseControl = new CruiseControl(locoController, config, new UnityClock())
            {
                Accelerator = new PredictiveAcceleration(),
                Decelerator = new PredictiveDeceleration()
            };

            systemManager = new SystemManager();
            ShiftSystem shiftSystem = new ShiftSystem(locoController);
            LocoStatsSystem locoStatsSystem = new LocoStatsSystem(locoController, 0.5f, Time.fixedDeltaTime);
            systemManager.AddSystem(shiftSystem);
            systemManager.AddSystem(locoStatsSystem);

            updateAccumulator = 0;
            Loaded?.Invoke(this, null);
            window.CruiseControl = cruiseControl;
        }

        public void Unload()
        {
            logger.Info("Unload");
            if (!loaded)
            {
                logger.Warn("Tried to unload DriverAssist, but it's not loaded");
                return;
            }

            PlayerManager.CarChanged -= OnCarChanged;

            loaded = false;
            Unloaded?.Invoke(this, null);

            if (window != null)
            {
                // Loaded -= window.OnLoad;
                // Unloaded -= window.OnUnload;
                EnterLoco -= window.Show;
                ExitLoco -= window.Hide;
                if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged -= window.OnPhotoModeChanged;
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
                ExitLoco.Invoke();
                if (locoController != null)
                {
                    locoController.UpdateLocomotive(NullTrainCarWrapper.Instance);
                    locoController.Components.LocoSettings = null;
                }
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
                LocoSettings settings = config.LocoSettings[locoController?.LocoType ?? ""];
                if (locoController != null)
                {
                    locoController.UpdateLocomotive(train);
                    locoController.Components.LocoSettings = null;
                    if (settings != null)
                    {
                        locoController.Components.LocoSettings = settings;
                    }
                    EnterLoco.Invoke(locoController);
                }
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
