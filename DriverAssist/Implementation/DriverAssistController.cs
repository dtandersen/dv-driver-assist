using System;
using DriverAssist.Cruise;
using DriverAssist.ECS;
using DV.Logic.Job;
using DV.Utils;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public delegate void EnterLocoHandler(LocoEntity locoController);
    public delegate void NoArgsHandler();

    public class DriverAssistController
    {
        private const int CC_SPEED_STEP = 5;

        private LocoEntity? locoController;
        private CruiseControl? cruiseControl;
        private readonly UnifiedSettings config;
        private float updateAccumulator;
        private bool loaded = false;
        private GameObject? gameObject;
        private CruiseControlWindow? cruiseControlWindow;
        private StatsWindow? statsWindow;
        private JobWindow? jobWindow;
        private readonly Logger logger = LogFactory.GetLogger(typeof(DriverAssistController));
        private SystemManager? systemManager;
        private JobSystem? jobSystem;

        public event EventHandler Loaded = delegate { };
        public event EventHandler Unloaded = delegate { };
        public event EnterLocoHandler EnterLoco = delegate { };
        public event NoArgsHandler ExitLoco = delegate { };

        public PlayerCameraSwitcher? PlayerCameraSwitcher { get { return SingletonBehaviour<PlayerCameraSwitcher>.Instance; } }

        public static DriverAssistController? Instance { get; internal set; }

        public DriverAssistController(UnifiedSettings config)
        {
            this.config = config;
        }

        public void Init()
        {
            Instance = this;
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

            systemManager?.Update();

            if (locoController?.IsLoco ?? false)
            {
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
            logger.Info("Register jobs");
            loaded = true;
            logger.Info("Load");

            gameObject = new GameObject("DriverAssistWindow");
            statsWindow = gameObject.AddComponent<StatsWindow>();
            statsWindow.Config = config;

            cruiseControlWindow = gameObject.AddComponent<CruiseControlWindow>();
            cruiseControlWindow.Config = config;

            jobWindow = gameObject.AddComponent<JobWindow>();
            jobWindow.Config = config;
            EnterLoco += statsWindow.Show;
            ExitLoco += statsWindow.Hide;
            if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged += statsWindow.OnPhotoModeChanged;

            EnterLoco += cruiseControlWindow.Show;
            ExitLoco += cruiseControlWindow.Hide;
            if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged += cruiseControlWindow.OnPhotoModeChanged;

            EnterLoco += jobWindow.Show;
            ExitLoco += jobWindow.Hide;
            if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged += jobWindow.OnPhotoModeChanged;

            PlayerManager.CarChanged += OnCarChanged;

            locoController = new LocoEntity(Time.fixedDeltaTime);

            cruiseControl = new CruiseControl(locoController, config, new UnityClock())
            {
                Accelerator = new PredictiveAcceleration(),
                Decelerator = new PredictiveDeceleration()
            };

            systemManager = new SystemManager();
            ShiftSystem shiftSystem = new ShiftSystem(locoController);
            LocoStatsSystem locoStatsSystem = new LocoStatsSystem(locoController, 0.5f, Time.fixedDeltaTime);
            jobSystem = new JobSystem();
            jobSystem.UpdateTask += jobWindow.OnJobAccepted;
            jobSystem.RemoveTask += jobWindow.OnJobRemoved;

            JobsManager jm = SingletonBehaviour<JobsManager>.Instance;
            foreach (Job job in jm.currentJobs)
            {
                OnRegisterJob(job);
            }

            systemManager.AddSystem(shiftSystem);
            systemManager.AddSystem(locoStatsSystem);
            systemManager.AddSystem(jobSystem);

            updateAccumulator = 0;
            Loaded?.Invoke(this, null);
            statsWindow.CruiseControl = cruiseControl;
            cruiseControlWindow.CruiseControl = cruiseControl;
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

            if (statsWindow != null)
            {
                EnterLoco -= statsWindow.Show;
                ExitLoco -= statsWindow.Hide;
                if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged -= statsWindow.OnPhotoModeChanged;
            }

            if (cruiseControlWindow != null)
            {
                EnterLoco -= cruiseControlWindow.Show;
                ExitLoco -= cruiseControlWindow.Hide;
                if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged -= cruiseControlWindow.OnPhotoModeChanged;
            }

            if (jobWindow != null)
            {
                EnterLoco -= jobWindow.Show;
                ExitLoco -= jobWindow.Hide;
                if (PlayerCameraSwitcher != null) PlayerCameraSwitcher.externalCamera.PhotoModeChanged -= jobWindow.OnPhotoModeChanged;
            }

            if (jobSystem != null)
            {
                jobSystem.UpdateTask = delegate { };
                jobSystem.RemoveTask = delegate { };
            }

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
                LocoSettings settings = config.LocoSettings[locoController?.Type ?? ""];
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

        bool IsKeyPressed(KeyMatcher keyMatcher)
        {
            return keyMatcher.IsKeyPressed();
        }

        /// Player has entered or exited a car
        public void OnCarChanged(TrainCar? enteredCar)
        {
            logger.Info($"OnCarChanged {enteredCar?.carType.ToString() ?? "null"}");
            ChangeCar(enteredCar);
        }

        internal void OnRegisterJob(Job job)
        {
            logger.Info($"OnRegisterJob {job.ID} {job.chainData.chainOriginYardId} -> {job.chainData.chainDestinationYardId}");
            jobSystem?.AddJob(new DVJobWrapper(job));
        }

        internal void OnUnregisterJob(Job job)
        {
            logger.Info($"OnUnregisterJob {job.chainData.chainOriginYardId} -> {job.chainData.chainDestinationYardId}");
            jobWindow?.OnJobRemoved(job.ID);
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
