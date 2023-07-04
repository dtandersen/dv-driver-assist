using System;
using CruiseControlPlugin.Algorithm;

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
                minSpeed = desiredSpeed + offset - diff;
                maxSpeed = desiredSpeed + offset + diff;
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

        private LocoController loco;
        private float minSpeed;
        private float maxSpeed;
        private float offset = -2.5f;
        private float diff = 2.5f;
        private float desiredSpeed = 0;
        private float lastThrottle;
        private float lastTrainBrake;
        private float lastIndBrake;

        public CruiseControl(LocoController loco)
        {
            this.loco = loco;
        }

        public void Tick()
        {
            if (IsControlsChanged())
            {
                Log($"controls changed lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
                Enabled = false;
            }

            if (!Enabled)
            {
                return;
            }
            float estspeed = loco.Acceleration * 10;
            Log($"controls lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
            if (loco.Speed + estspeed < minSpeed)
            {
                // Debug.Log($"speed={loco.Speed} minspeed={minSpeed}");
                Accelerator.Tick(loco);
                minSpeed = desiredSpeed + offset;
                Log($"Accelerating to {minSpeed}");
            }
            else if (loco.Speed + estspeed > maxSpeed)
            {
                Decelerator.Tick(loco);
                maxSpeed = desiredSpeed + offset;
                Log($"Decelerating to {maxSpeed}");
            }
            else
            {
                loco.Throttle = 0;
                loco.TrainBrake = 0;
                minSpeed = desiredSpeed + offset - diff;
                maxSpeed = desiredSpeed + offset + diff;
                Log($"Idle");
            }

            lastThrottle = loco.Throttle;
            lastTrainBrake = loco.TrainBrake;
            lastIndBrake = loco.IndBrake;
            Log($"controls lastThrottle={lastThrottle} loco.Throttle={loco.Throttle} lastTrainBrake={lastTrainBrake} loco.TrainBrake={loco.TrainBrake} lastIndBrake={lastIndBrake} loco.IndBrake={loco.IndBrake}");
        }

        private void Log(string message)
        {
            LoggerSingleton.Instance.Info(message);
        }

        private bool IsControlsChanged()
        {
            return
                changed(lastThrottle, loco.Throttle) ||
                changed(lastTrainBrake, loco.TrainBrake) ||
                changed(lastIndBrake, loco.IndBrake);
        }

        bool changed(float v1, float v2)
        {
            return Math.Abs(v1 - v2) > .1f;
        }
    }
}