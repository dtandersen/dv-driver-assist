using System;
using System.Collections.Generic;
using System.Diagnostics;
using DriverAssist.Cruise;
using DriverAssist.ECS;
using DriverAssist.Localization;
using DV.Logic.Job;
using DV.Utils;
using I2.Loc;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public delegate void EnterLocoHandler(LocoEntity locoController);
    public delegate void NoArgsHandler();

    public class DriverAssistController
    {
        private const int CC_SPEED_STEP = 5;

        private LocoEntity? locoEntity;
        private CruiseControl? cruiseControl;
        private readonly UnifiedSettings config;
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


        internal void Load()
        {
            TranslationManager.SetLangage(LocalizationManager.CurrentLanguage);

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

            locoEntity = new LocoEntity(Time.fixedDeltaTime);
            EntityManager.Instance.Loco = locoEntity;

            cruiseControl = new CruiseControl(config, new UnityClock(), EntityManager.Instance)
            {
                Accelerator = new PredictiveAcceleration(),
                Decelerator = new PredictiveDeceleration()
            };

            systemManager = new SystemManager();
            ShiftSystem shiftSystem = new ShiftSystem(locoEntity);
            LocoStatsSystem locoStatsSystem = new LocoStatsSystem(locoEntity, 0.5f, Time.fixedDeltaTime);
            jobSystem = new JobSystem();
            jobSystem.JobUpdated += jobWindow.OnAddJob;
            jobSystem.JobRemoved += jobWindow.OnRemoveJob;

            JobsManager jm = SingletonBehaviour<JobsManager>.Instance;
            foreach (Job job in jm.currentJobs)
            {
                OnRegisterJob(job);
            }

            systemManager.AddSystem(new ControlsChangedSystem(EntityManager.Instance));
            systemManager.AddSystem(shiftSystem);
            systemManager.AddSystem(locoStatsSystem);
            systemManager.AddSystem(jobSystem);
            systemManager.AddSystem(new DelayedSystem(cruiseControl, 1, 1f / 60f));
            systemManager.AddSystem(new LastControlsSystem(EntityManager.Instance));

            // updateAccumulator = 0;
            Loaded?.Invoke(this, null);
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
                jobSystem.JobUpdated = delegate { };
                jobSystem.JobRemoved = delegate { };
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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            systemManager?.Update();

            if (locoEntity?.IsLoco ?? false)
            {
                locoEntity.UpdateStats(Time.fixedDeltaTime);
            }
            stopWatch.Stop();
            if (new System.Random().NextDouble() > .9f)
                statsWindow?.OnFrameTime(stopWatch.Elapsed.TotalMilliseconds);
        }

        public void OnUpdate()
        {
            if (!loaded) return;

            if (IsKeyPressed(config.ToggleKeys))
            {
                if (locoEntity == null) return;
                if (locoEntity.Components.CruiseControl == null)
                {
                    locoEntity.Components.CruiseControl = CruiseControlComponent.Make(0, config.Diff, config.Offset);
                }
                else
                {
                    locoEntity.Components.CruiseControl = null;
                }
            }
            if (IsKeyPressed(config.AccelerateKeys))
            {
                if (locoEntity == null) return;
                if (!locoEntity.Components.CruiseControl.HasValue) return;

                CruiseControlComponent c = locoEntity.Components.CruiseControl.Value;
                locoEntity.Components.CruiseControl = CruiseControlComponent.Make((int)(c.DesiredSpeed + CC_SPEED_STEP), config.Diff, config.Offset);
            }
            if (IsKeyPressed(config.DecelerateKeys))
            {
                if (locoEntity == null) return;
                if (!locoEntity.Components.CruiseControl.HasValue) return;

                CruiseControlComponent c = locoEntity.Components.CruiseControl.Value;
                locoEntity.Components.CruiseControl = CruiseControlComponent.Make((int)(c.DesiredSpeed - CC_SPEED_STEP), config.Diff, config.Offset);
            }

            if (locoEntity != null)
            {
                if (IsKeyPressed(config.Upshift))
                {
                    locoEntity.Upshift();
                }
                if (IsKeyPressed(config.Downshift))
                {
                    locoEntity.Downshift();
                }
                if (IsKeyPressed(config.DumpPorts))
                {
                    foreach (var port in locoEntity.Ports)
                    {
                        logger.Info($"{port}");
                    }
                }
            }
        }

        public void ChangeCar(TrainCar? trainCar)
        {
            logger.Info($"ChangeCar {trainCar?.carType.ToString() ?? "null"}");

            LoadIfNotLoaded();
            if (trainCar == null || !trainCar.IsLoco)
            {
                ExitLoco.Invoke();
                if (locoEntity != null)
                {
                    locoEntity.UpdateLocomotive(NullTrainCarWrapper.Instance);
                    locoEntity.Components.LocoSettings = null;

                }
                logger.Info($"Exited train car");
                if (locoEntity != null) locoEntity.Components.CruiseControl = null;
            }
            else
            {
                DVTrainCarWrapper train = new(trainCar);
                logger.Info($"Entered train car {trainCar?.carType.ToString() ?? "null"}");
                LocoSettings settings = config.LocoSettings[locoEntity?.Type ?? ""];
                if (locoEntity != null)
                {
                    locoEntity.UpdateLocomotive(train);
                    locoEntity.Components.LocoSettings = null;
                    if (settings != null)
                    {
                        locoEntity.Components.LocoSettings = settings;
                    }
                    EnterLoco.Invoke(locoEntity);
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
            JobWrapper jw = new DVJobWrapper(job);
            void LogTasks(List<TaskWrapper> tasks, string prefix)
            {
                int i = 1;
                foreach (TaskWrapper task in tasks)
                {
                    logger.Info($"{prefix}{i} - Type={(TaskType)task.Type} Source={task.Source} Dest={task.Destination}");
                    if (task.IsParallel || task.IsSequential)
                    {
                        logger.Info($"--- BEGIN SUBTASKS ---");
                        LogTasks(task.Tasks, $"{prefix}{i}.");
                        logger.Info($"--- END SUBTASKS ---");
                    }
                    i++;
                }

            }
            LogTasks(jw.Tasks, "");
            jobSystem?.AddJob(jw);
        }

        internal void OnUnregisterJob(Job job)
        {
            logger.Info($"OnUnregisterJob {job.chainData.chainOriginYardId} -> {job.chainData.chainDestinationYardId}");
            jobSystem?.RemoveJob(job.ID);
        }
    }
}
