using System;

namespace DriverAssist.Cruise
{
    public class PredictiveAcceleration : CruiseControlAlgorithm
    {
        public float DesiredSpeed { get; set; }
        public float DesiredTorque { get; set; }

        float lastTorque = 0;
        float lastAmps = 0;
        float step = 1f / 11f;
        // private DefaultAccelerationAlgo accelerate;
        // private DefaultDecelerationAlgo decelerate;

        public PredictiveAcceleration()
        {
            DesiredTorque = 25000;
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            // Debug.Log($"DesiredSpeed={DesiredSpeed}");

            float reverser = loco.Reverser;
            float speed = loco.RelativeSpeed;
            float desiredSpeed = DesiredSpeed;
            // if (reverser == 1)
            // {
            //     desiredSpeed = DesiredSpeed;
            // }
            // else
            // {
            //     desiredSpeed = -DesiredSpeed;
            // }
            float throttle = loco.Throttle;
            float torque = loco.Torque;
            float temperature = loco.Temperature;
            float ampDelta = loco.Amps - lastAmps;
            float throttleResult;
            float projectedAmps = loco.Amps + ampDelta * 3f;
            if (speed < 5)
            {
                throttleResult = step;
            }
            else if (speed > desiredSpeed)
            {
                throttleResult = 0;
            }
            else if (loco.Temperature > 100)
            {
                throttleResult = throttle - step;
            }
            else if (projectedAmps > 550)
            {
                throttleResult = throttle - step;
            }
            else if (projectedAmps < 450)
            {
                throttleResult = throttle + step;
            }
            else if (loco.Amps > 600 && !(loco.Amps < lastAmps))
            {
                throttleResult = throttle - step;
            }
            else
            {
                throttleResult = throttle;
            }

            loco.Throttle = throttleResult;
            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            lastTorque = torque;
            lastAmps = loco.Amps;
        }
    }
}
