using System;
using System.Collections.Generic;
using DriverAssist.Localization;

namespace DriverAssist.Cruise
{
#pragma warning disable IDE1006
    public interface Clock
#pragma warning restore IDE1006
    {
        public float Time2 { get; }
    }

    public class CruiseControl
    {
        private readonly Logger logger;
        private float desiredSpeed = 0;
        public float DesiredSpeed
        {
            get { return desiredSpeed; }
            set
            {
                desiredSpeed = value;
                positiveDesiredSpeed = Math.Abs(value);
                minSpeed = positiveDesiredSpeed + config.Offset - config.Diff;
                maxSpeed = positiveDesiredSpeed + config.Offset + config.Diff;
            }
        }

        public CruiseControlAlgorithm Accelerator { get; set; }
        public CruiseControlAlgorithm Decelerator { get; set; }

        bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                lastThrottle = loco.Throttle;
                lastTrainBrake = loco.TrainBrake;
                lastIndBrake = loco.IndBrake;
            }
        }

        public string Status { get; internal set; }

        private readonly LocoController loco;
        private float minSpeed;
        private float maxSpeed;
        private float positiveDesiredSpeed;
        private float lastThrottle;
        private float lastTrainBrake;
        private float lastIndBrake;
        private readonly CruiseControlSettings config;
        private CruiseControlContext context;
        private readonly Translation localization;
        private readonly Clock clock;

        public CruiseControl(LocoController loco, CruiseControlSettings config, Clock clock)
        {
            logger = LogFactory.GetLogger("CruiseControl");
            localization = TranslationManager.Current;
            this.loco = loco;
            this.config = config;
            Accelerator = CreateAlgo(config.Acceleration);
            Decelerator = CreateAlgo(config.Deceleration);
            this.clock = clock;
            Status = "";
            context = new CruiseControlContext(config.LocoSettings[loco.LocoType], loco);
        }

        private CruiseControlAlgorithm CreateAlgo(string name)
        {
            Type type = Type.GetType(name, true);
            CruiseControlAlgorithm instance = (CruiseControlAlgorithm)Activator.CreateInstance(type);
            return instance;
        }

        public void Tick()
        {
            // PluginLoggerSingleton.Instance.Info($"Tick minSpeed={minSpeed} maxSpeed={maxSpeed} loco.RelativeSpeedKmh={loco.RelativeSpeedKmh}");

            try
            {
                context = new CruiseControlContext(config.LocoSettings[loco.LocoType], loco)
                {
                    Time = clock.Time2
                };
            }
            catch (KeyNotFoundException)
            {
                Status = string.Format(localization.CC_UNSUPPORTED, loco.LocoType);
                // Status = String.Format("No settings found for {0}", loco.LocoType);
                return;
            }

            if (IsControlsChanged())
            {
                logger.Info($"Disabled cruise control lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
                Enabled = false;
            }

            if (!Enabled)
            {
                Status = string.Format(localization.CC_DISABLED);
                // Status = String.Format("Disabled");
                return;
            }

            if (loco.Reverser == 0.5f)
            {
                Status = string.Format(localization.CC_WARNING_NEUTRAL);
                // Status = String.Format("Idle: Reverser is in neutral");
                loco.Throttle = 0;
                return;
            }

            // float estspeed = loco.Acceleration * 10;
            float estspeed = 0;

            if (positiveDesiredSpeed == 0)
            {
                Status = string.Format(localization.CC_STOPPING);
                // Status = String.Format("Stop");
                loco.Throttle = 0;
                if (!(loco.LocoType == LocoType.DM3))
                    loco.TrainBrake = 1;
                else
                    loco.TrainBrake = .666f;
            }
            else if (IsWrongDirection)
            {
                Status = string.Format(localization.CC_CHANGING_DIRECTION);
                // Status = String.Format("Direction change");
                loco.Throttle = 0;
                if (!(loco.LocoType == LocoType.DM3))
                    loco.TrainBrake = 1;
                else
                    loco.TrainBrake = .666f;
                if (loco.Reverser == 0 && DesiredSpeed > 0 && Math.Abs(loco.SpeedKmh) < 0.1f)
                {
                    loco.Reverser = 1f;
                }
                if (loco.Reverser == 1 && DesiredSpeed < 0 && Math.Abs(loco.SpeedKmh) < 0.1f)
                {
                    loco.Reverser = 0f;
                }
            }
            else if (loco.RelativeSpeedKmh + estspeed < minSpeed)
            {
                // PluginLoggerSingleton.Instance.Info($"Accelerate minSpeed={minSpeed} maxSpeed={maxSpeed} loco.RelativeSpeedKmh={loco.RelativeSpeedKmh}");
                context.DesiredSpeed = Math.Abs(DesiredSpeed);
                Accelerator.Tick(context);
                minSpeed = positiveDesiredSpeed + config.Offset;
                maxSpeed = positiveDesiredSpeed + config.Offset + config.Diff;
                Status = string.Format(localization.CC_ACCELERATING, minSpeed);
                // Status = String.Format("Accelerating to {0} km/h", minSpeed);
            }
            else if (loco.RelativeSpeedKmh + estspeed > maxSpeed)
            {
                // PluginLoggerSingleton.Instance.Info($"Decellerate minSpeed={minSpeed} maxSpeed={maxSpeed} loco.RelativeSpeedKmh={loco.RelativeSpeedKmh}");
                context.DesiredSpeed = maxSpeed;
                Decelerator.Tick(context);
                maxSpeed = positiveDesiredSpeed + config.Offset;
                minSpeed = positiveDesiredSpeed + config.Offset - config.Diff;
                // minSpeed = positiveDesiredSpeed - config.Offset;
                // Status = String.Format("Decelerating to {0} km/h", maxSpeed);
                Status = string.Format(localization.CC_DECELERATING, maxSpeed);
            }
            else
            {
                // Status = String.Format("Coast");
                // PluginLoggerSingleton.Instance.Info($"Coast minSpeed={minSpeed} maxSpeed={maxSpeed} loco.RelativeSpeedKmh={loco.RelativeSpeedKmh}");

                Status = string.Format(localization.CC_COASTING);
                loco.Throttle = 0;
                loco.TrainBrake = 0;
                minSpeed = positiveDesiredSpeed + config.Offset - config.Diff;
                maxSpeed = positiveDesiredSpeed + config.Offset + config.Diff;
            }

            lastThrottle = loco.Throttle;
            lastTrainBrake = loco.TrainBrake;
            lastIndBrake = loco.IndBrake;
        }

        private bool IsWrongDirection
        {
            get
            {
                // PluginLoggerSingleton.Instance.Info($"Reverser={context.LocoController.Reverser} DesiredSpeed={DesiredSpeed}");
                return context.LocoController.Reverser == 1 && DesiredSpeed < 0 || context.LocoController.Reverser == 0 && DesiredSpeed > 0;
            }
        }

        // private void Log(string message)
        // {
        //     DriverAssistLogger.Instance.Info(message);
        // }

        private bool IsControlsChanged()
        {
            return
                Changed(lastTrainBrake, context.LocoController.TrainBrake, 1f / 11f) ||
                Changed(lastIndBrake, context.LocoController.IndBrake, 1f / 11f);
        }

        bool Changed(float v1, float v2, float amount)
        {
            return Math.Abs(v1 - v2) > amount;
        }
    }
}
