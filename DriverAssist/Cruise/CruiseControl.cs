using System;
using DriverAssist.ECS;
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
                if (entityManager.Loco == null) return;

                LocoEntity loco = entityManager.Loco;
                lastThrottle = loco.Throttle;
                lastTrainBrake = loco.TrainBrake;
                lastIndBrake = loco.IndBrake;
            }
        }

        public string Status { get; internal set; }

        private float minSpeed;
        private float maxSpeed;
        private float positiveDesiredSpeed;
        private float lastThrottle;
        private float lastTrainBrake;
        private float lastIndBrake;
        private readonly CruiseControlSettings config;
        private readonly Translation localization;
        private readonly Clock clock;
        private readonly EntityManager entityManager;

        public CruiseControl(CruiseControlSettings config, Clock clock, EntityManager entityManager)
        {
            logger = LogFactory.GetLogger("CruiseControl");
            localization = TranslationManager.Current;
            this.config = config;
            Accelerator = CreateAlgo(config.Acceleration);
            Decelerator = CreateAlgo(config.Deceleration);
            this.clock = clock;
            this.entityManager = entityManager;
            Status = "";
        }

        private CruiseControlAlgorithm CreateAlgo(string name)
        {
            Type type = Type.GetType(name, true);
            CruiseControlAlgorithm instance = (CruiseControlAlgorithm)Activator.CreateInstance(type);
            return instance;
        }

        public void Tick()
        {
            if (entityManager.Loco == null) return;
            LocoEntity loco = entityManager.Loco;

            if (loco.Components.LocoSettings == null)
            {
                Status = string.Format(localization.CC_UNSUPPORTED, loco.Type);
                return;

            }
            CruiseControlContext context = new CruiseControlContext(loco.Components.LocoSettings, loco)
            {
                Time = clock.Time2
            };

            if (IsControlsChanged(context))
            {
                logger.Info($"Disabled cruise control lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
                Enabled = false;
            }

            if (!Enabled)
            {
                Status = string.Format(localization.CC_DISABLED);
                return;
            }

            if (loco.Reverser == 0.5f)
            {
                Status = string.Format(localization.CC_WARNING_NEUTRAL);
                loco.Throttle = 0;
                return;
            }

            float estspeed = 0;

            if (positiveDesiredSpeed == 0)
            {
                Status = string.Format(localization.CC_STOPPING);
                loco.Throttle = 0;
                if (!(loco.Type == LocoType.DM3))
                    loco.TrainBrake = 1;
                else
                    loco.TrainBrake = .666f;
            }
            else if (IsWrongDirection(context))
            {
                Status = string.Format(localization.CC_CHANGING_DIRECTION);
                loco.Throttle = 0;
                if (!(loco.Type == LocoType.DM3))
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
                context.DesiredSpeed = Math.Abs(DesiredSpeed);
                Accelerator.Tick(context);
                minSpeed = positiveDesiredSpeed + config.Offset;
                maxSpeed = positiveDesiredSpeed + config.Offset + config.Diff;
                Status = string.Format(localization.CC_ACCELERATING, minSpeed);
            }
            else if (loco.RelativeSpeedKmh + estspeed > maxSpeed)
            {
                context.DesiredSpeed = maxSpeed;
                Decelerator.Tick(context);
                maxSpeed = positiveDesiredSpeed + config.Offset;
                minSpeed = positiveDesiredSpeed + config.Offset - config.Diff;
                Status = string.Format(localization.CC_DECELERATING, maxSpeed);
            }
            else
            {
                Status = string.Format(localization.CC_COASTING);
                loco.Throttle = 0;
                loco.TrainBrake = 0;
                loco.IndBrake = 0;
                if (loco.BrakeCylinderPressure > 0) loco.ReleaseBrakeCylinder();
                minSpeed = positiveDesiredSpeed + config.Offset - config.Diff;
                maxSpeed = positiveDesiredSpeed + config.Offset + config.Diff;
            }

            lastThrottle = loco.Throttle;
            lastTrainBrake = loco.TrainBrake;
            lastIndBrake = loco.IndBrake;
        }

        private bool IsWrongDirection(CruiseControlContext context)
        {
            return context.LocoController.Reverser == 1 && DesiredSpeed < 0 || context.LocoController.Reverser == 0 && DesiredSpeed > 0;
        }

        private bool IsControlsChanged(CruiseControlContext context)
        {
            return
                Changed(lastTrainBrake, context.LocoController.TrainBrake, 1f / 11f) ||
                Changed(lastIndBrake, context.LocoController.IndBrake, 1f / 11f);
        }

        private bool Changed(float v1, float v2, float amount)
        {
            return Math.Abs(v1 - v2) > amount;
        }
    }
}
