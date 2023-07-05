using System;
using System.Collections.Generic;
using CruiseControlPlugin.Algorithm;
using MyFirstPlugin;

namespace CruiseControlPlugin
{
    public class CruiseControl
    {
        bool reverse;
        public float DesiredSpeed
        {
            get { return desiredSpeed; }
            set
            {
                if (value < 0)
                {
                    reverse = true;
                }
                else
                {
                    reverse = false;
                }

                desiredSpeed = value;
                positiveDesiredSpeed = Math.Abs(value);
                minSpeed = positiveDesiredSpeed + offset - diff;
                maxSpeed = positiveDesiredSpeed + offset + diff;
                Accelerator.DesiredSpeed = positiveDesiredSpeed;
                Decelerator.DesiredSpeed = positiveDesiredSpeed;
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
        private float offset = -2.5f;
        private float diff = 2.5f;
        private float desiredSpeed = 0;
        private float positiveDesiredSpeed;
        private float lastThrottle;
        private float lastTrainBrake;
        private float lastIndBrake;

        public CruiseControl(LocoController loco, CruiseControlConfig bepinexCruiseControlConfig)
        {
            this.loco = loco;
        }

        public void Tick()
        {
            if (IsControlsChanged())
            {
                Log($"Disabled cruise control lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
                Enabled = false;
            }

            if (!Enabled)
            {
                Status = "Disabled";
                return;
            }

            if (loco.Reverser == 0.5f)
            {
                Status = "Idle: Reverser is in neutral";
                loco.Throttle = 0;
                return;
            }

            // float estspeed = loco.Acceleration * 10;
            float estspeed = 0;

            if (positiveDesiredSpeed == 0)
            {
                Status = "Stop";
                loco.Throttle = 0;
                loco.TrainBrake = 1;
            }
            else if (IsWrongDirection)
            {
                Status = "Direction change";
                loco.Throttle = 0;
                loco.TrainBrake = 1;
                if (loco.Reverser == 0 && DesiredSpeed > 0 && loco.Speed == 0)
                {
                    loco.Reverser = 1f;
                }
                if (loco.Reverser == 1 && DesiredSpeed < 0 && loco.Speed == 0)
                {
                    loco.Reverser = 0f;
                }
            }
            else if (loco.PositiveSpeed + estspeed < minSpeed)
            {
                Accelerator.Tick(loco);
                minSpeed = positiveDesiredSpeed + offset;
                Status = $"Accelerating to {minSpeed} km/h";
            }
            else if (loco.PositiveSpeed + estspeed > maxSpeed)
            {
                Decelerator.DesiredSpeed = maxSpeed;
                Decelerator.Tick(loco);
                maxSpeed = positiveDesiredSpeed + offset;
                Status = $"Decelerating to {maxSpeed} km/h";
            }
            else
            {
                Status = "Coast";
                loco.Throttle = 0;
                loco.TrainBrake = 0;
                minSpeed = positiveDesiredSpeed + offset - diff;
                maxSpeed = positiveDesiredSpeed + offset + diff;
            }

            lastThrottle = loco.Throttle;
            lastTrainBrake = loco.TrainBrake;
            lastIndBrake = loco.IndBrake;
        }

        private bool IsWrongDirection
        {
            get
            {
                return loco.Reverser == 1 && DesiredSpeed < 0 || loco.Reverser == 0 && DesiredSpeed > 0;
            }
        }

        private void Log(string message)
        {
            LoggerSingleton.Instance.Info(message);
        }

        private bool IsControlsChanged()
        {
            return
                // changed(lastThrottle, loco.Throttle) ||
                changed(lastTrainBrake, loco.TrainBrake, 1f / 11f) ||
                changed(lastIndBrake, loco.IndBrake, 1f / 11f);
        }

        bool changed(float v1, float v2, float amount)
        {
            return Math.Abs(v1 - v2) > amount;
        }
    }
}