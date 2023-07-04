using System;
using System.Collections.Generic;
using CruiseControlPlugin.Algorithm;
using MyFirstPlugin;

namespace CruiseControlPlugin
{
    public class CruiseControl
    {
        public float DesiredSpeed
        {
            get { return desiredSpeed; }
            set
            {
                desiredSpeed = value;
                minSpeed = Math.Abs(desiredSpeed) + offset - diff;
                maxSpeed = Math.Abs(desiredSpeed) + offset + diff;
                Accelerator.DesiredSpeed = value;
                Decelerator.DesiredSpeed = value;
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
            float desiredForwardSpeed;
            float locoForwardSpeed;
            if (loco.Reverser == 1)
            {
                desiredForwardSpeed = desiredSpeed;
                locoForwardSpeed = loco.Speed;
            }
            else if (loco.Reverser == 0)
            {
                desiredForwardSpeed = -desiredSpeed;
                locoForwardSpeed = -loco.Speed;
            }
            else
            {
                return;
            }

            if (IsWrongDirection)
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
            else if (locoForwardSpeed + estspeed < minSpeed)
            {
                // Debug.Log($"speed={loco.Speed} minspeed={minSpeed}");
                Accelerator.Tick(loco);
                minSpeed = desiredForwardSpeed + offset;
                Status = $"Accelerating to {minSpeed} km/h";
                // Log($"Accelerating to {minSpeed}");
            }
            else if (locoForwardSpeed + estspeed > maxSpeed)
            {
                Decelerator.Tick(loco);
                maxSpeed = desiredForwardSpeed + offset;
                Status = $"Decelerating to {maxSpeed} km/h";
                // Log($"Decelerating to {maxSpeed}");
            }
            else
            {
                Status = "Cruising";
                loco.Throttle = 0;
                loco.TrainBrake = 0;
                minSpeed = desiredSpeed + offset - diff;
                maxSpeed = desiredSpeed + offset + diff;
                // Log($"Idle");
            }

            lastThrottle = loco.Throttle;
            lastTrainBrake = loco.TrainBrake;
            lastIndBrake = loco.IndBrake;
            // Log($"controls lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
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