using System;
using DriverAssist.ECS;
using DriverAssist.Localization;

namespace DriverAssist.Cruise
{
    public interface Clock
    {
        public float Time2 { get; }
    }

    public class CruiseControl : BaseSystem
    {
        public string Status { get; internal set; }
        public CruiseControlAlgorithm Accelerator { get; set; }
        public CruiseControlAlgorithm Decelerator { get; set; }

        // private readonly Logger logger;
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

        override public void OnUpdate()
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
                loco.Components.CruiseControl = null;
                if (loco.Components.LastControls == null) return;
                logger.Info($"Disabled cruise control lastTrainBrake={loco.Components.LastControls.Value.TrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={loco.Components.LastControls.Value.IndBrake} loco.IndBrake={loco.IndBrake}");
            }

            if (loco.Components.CruiseControl == null)
            {
                Status = string.Format(localization.CC_DISABLED);
                return;
            }

            float desiredSpeed = loco.Components.CruiseControl.Value.DesiredSpeed;
            float positiveDesiredSpeed = Math.Abs(desiredSpeed);
            float minSpeed = loco.Components.CruiseControl.Value.MinSpeed;
            float maxSpeed = loco.Components.CruiseControl.Value.MaxSpeed;

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
            else if (IsWrongDirection(context, desiredSpeed))
            {
                Status = string.Format(localization.CC_CHANGING_DIRECTION);
                loco.Throttle = 0;
                if (!(loco.Type == LocoType.DM3))
                    loco.TrainBrake = 1;
                else
                    loco.TrainBrake = .666f;
                if (loco.Reverser == 0 && desiredSpeed > 0 && Math.Abs(loco.SpeedKmh) < 0.1f)
                {
                    loco.Reverser = 1f;
                }
                if (loco.Reverser == 1 && desiredSpeed < 0 && Math.Abs(loco.SpeedKmh) < 0.1f)
                {
                    loco.Reverser = 0f;
                }
            }
            else if (loco.RelativeSpeedKmh + estspeed < minSpeed)
            {
                context.DesiredSpeed = Math.Abs(desiredSpeed);
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

            loco.Components.CruiseControl = new CruiseControlComponent()
            {
                DesiredSpeed = desiredSpeed,
                MinSpeed = minSpeed,
                MaxSpeed = maxSpeed
            };
        }

        private bool IsWrongDirection(CruiseControlContext context, float desiredSpeed)
        {
            return context.LocoController.Reverser == 1 && desiredSpeed < 0 || context.LocoController.Reverser == 0 && desiredSpeed > 0;
        }

        private bool IsControlsChanged(CruiseControlContext context)
        {
            if (context.LocoController.Components.ControlsChanged == null) return false;

            return context.LocoController.Components.ControlsChanged.Value;
        }
    }
}
