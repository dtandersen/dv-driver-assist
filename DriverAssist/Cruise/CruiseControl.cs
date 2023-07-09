using System;
using System.Collections.Generic;
using DriverAssist.Localization;

namespace DriverAssist.Cruise
{
    public class CruiseControl
    {
        bool forward;
        private float desiredSpeed = 0;
        public float DesiredSpeed
        {
            get { return desiredSpeed; }
            set
            {
                if (value >= 0)
                {
                    forward = true;
                }
                else
                {
                    forward = false;
                }

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

        private LocoController loco;
        private float minSpeed;
        private float maxSpeed;
        private float positiveDesiredSpeed;
        private float lastThrottle;
        private float lastTrainBrake;
        private float lastIndBrake;
        private CruiseControlSettings config;
        private CruiseControlContext context;
        private Translation localization;

        public CruiseControl(LocoController loco, CruiseControlSettings config)
        {
            this.loco = loco;
            this.config = config;
            Accelerator = CreateAlgo(config.Acceleration);
            Decelerator = CreateAlgo(config.Deceleration);
        }

        private CruiseControlAlgorithm CreateAlgo(string name)
        {
            Type type = Type.GetType(name, true);
            CruiseControlAlgorithm instance = (CruiseControlAlgorithm)Activator.CreateInstance(type);
            localization = TranslationManager.Current;
            return instance;
        }

        public void Tick()
        {
            try
            {
                context = new CruiseControlContext(config.LocoSettings[loco.LocoType], loco);
            }
            catch (KeyNotFoundException)
            {
                Status = String.Format(localization.CC_UNSUPPORTED, loco.LocoType);
                // Status = String.Format("No settings found for {0}", loco.LocoType);
                return;
            }

            if (IsControlsChanged())
            {
                Log($"Disabled cruise control lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
                Enabled = false;
            }

            if (!Enabled)
            {
                Status = String.Format(localization.CC_DISABLED);
                // Status = String.Format("Disabled");
                return;
            }

            if (loco.Reverser == 0.5f)
            {
                Status = String.Format(localization.CC_WARNING_NEUTRAL);
                // Status = String.Format("Idle: Reverser is in neutral");
                loco.Throttle = 0;
                return;
            }

            // float estspeed = loco.Acceleration * 10;
            float estspeed = 0;

            if (positiveDesiredSpeed == 0)
            {
                Status = String.Format(localization.CC_STOPPING);
                // Status = String.Format("Stop");
                loco.Throttle = 0;
                loco.TrainBrake = 1;
            }
            else if (IsWrongDirection)
            {
                Status = String.Format(localization.CC_CHANGING_DIRECTION);
                // Status = String.Format("Direction change");
                loco.Throttle = 0;
                loco.TrainBrake = 1;
                if (loco.Reverser == 0 && DesiredSpeed > 0 && loco.SpeedKmh == 0)
                {
                    loco.Reverser = 1f;
                }
                if (loco.Reverser == 1 && DesiredSpeed < 0 && loco.SpeedKmh == 0)
                {
                    loco.Reverser = 0f;
                }
            }
            else if (loco.RelativeSpeedKmh + estspeed < minSpeed)
            {
                context.DesiredSpeed = Math.Abs(DesiredSpeed);
                Accelerator.Tick(context);
                minSpeed = positiveDesiredSpeed + config.Offset;
                Status = String.Format(localization.CC_ACCELERATING, minSpeed);
                // Status = String.Format("Accelerating to {0} km/h", minSpeed);
            }
            else if (loco.RelativeSpeedKmh + estspeed > maxSpeed)
            {
                context.DesiredSpeed = maxSpeed;
                Decelerator.Tick(context);
                maxSpeed = positiveDesiredSpeed + config.Offset;
                // Status = String.Format("Decelerating to {0} km/h", maxSpeed);
                Status = String.Format(localization.CC_DECELERATING, maxSpeed);
            }
            else
            {
                // Status = String.Format("Coast");
                Status = String.Format(localization.CC_COASTING);
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
                return context.LocoController.Reverser == 1 && DesiredSpeed < 0 || context.LocoController.Reverser == 0 && DesiredSpeed > 0;
            }
        }

        private void Log(string message)
        {
            PluginLoggerSingleton.Instance.Info(message);
        }

        private bool IsControlsChanged()
        {
            return
                changed(lastTrainBrake, context.LocoController.TrainBrake, 1f / 11f) ||
                changed(lastIndBrake, context.LocoController.IndBrake, 1f / 11f);
        }

        bool changed(float v1, float v2, float amount)
        {
            return Math.Abs(v1 - v2) > amount;
        }
    }
}
