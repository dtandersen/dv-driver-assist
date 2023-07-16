using System;

namespace DriverAssist.Cruise
{
    public class PredictiveAcceleration : CruiseControlAlgorithm
    {
        float lastAmps = 0;
        float lastTorque = 0;
        float lastTemperature = 0;
        float step = 1f / 11f;
        bool cooling = false;
        public float lastShift;
        PluginLogger logger;

        public PredictiveAcceleration()
        {
            logger = PluginLoggerSingleton.Instance;
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;

            float reverser = loco.Reverser;
            float speed = loco.RelativeSpeedKmh;
            float desiredSpeed = context.DesiredSpeed;
            float throttle = loco.Throttle;
            float torque = loco.RelativeTorque;
            float temperature = loco.Temperature;
            float ampDelta = loco.Amps - lastAmps;
            float throttleResult;
            float predictedAmps = loco.Amps;
            float acceleration = loco.RelativeAccelerationMs;
            float maxamps = context.Config.MaxAmps;
            float minTorque = context.Config.MinTorque;
            float amps = loco.Amps;
            float projectedTemperature = loco.Temperature + loco.TemperatureChange;
            float timeSinceShift = context.Time - lastShift;
            float operatingTemp = context.Config.MaxTemperature;
            float dangerTemp = context.Config.HillClimbTemp;
            float throttleAdj = 0;

            bool ampsdecreased = amps <= lastAmps;
            bool hillClimbActive = loco.AccelerationMs <= context.Config.HillClimbAccel;
            bool tempDecreasing = loco.TemperatureChange < 0;

            bool readyToShift =
                torque < minTorque
                && (torque < 10000 || torque <= lastTorque)
                && loco.RelativeAccelerationMs < 0.25f;

            log($"predictedAmps{predictedAmps} maxamps={maxamps} timeSinceShift={timeSinceShift}");
            log($"projectedTemperature={projectedTemperature} dangerTemp={dangerTemp}");

            if (speed > desiredSpeed)
            {
                log("Reached speed");

                throttleResult = 0;
                throttleAdj = -loco.Throttle;
            }
            else if (projectedTemperature >= dangerTemp)
            {
                log("dangerous temperature");
                throttleResult = throttle - step;
                throttleAdj = -step;
            }
            else if (
                projectedTemperature >= context.Config.MaxTemperature
                && !tempDecreasing
                && !hillClimbActive
                && timeSinceShift >= 3)
            {
                log("high temperature");
                throttleResult = throttle - step;
                throttleAdj = -step;
            }
            else if (
                predictedAmps >= maxamps
                && loco.IsElectric)
            {
                log("dangerous amps");
                throttleResult = throttle - step;
                throttleAdj = -step;
            }
            else if (
                acceleration >= context.Config.MaxAccel
                && loco.Throttle > step)
            {
                log("accelerating too fast");
                throttleResult = Math.Max(throttle - step, step);
                throttleAdj = throttleResult - loco.Throttle;
            }
            else if (
                acceleration < context.Config.MaxAccel
                && torque < context.Config.MinTorque
                && projectedTemperature < context.Config.MaxTemperature
                && timeSinceShift > 3
                )
            {
                log($"low accel acceleration={acceleration} context.Config.MaxAccel={context.Config.MaxAccel}");
                throttleResult = throttle + step;
                throttleAdj = step;
            }
            else if (
                torque < context.Config.MinTorque
                && acceleration < context.Config.MaxAccel
                && projectedTemperature < context.Config.MaxTemperature
                && timeSinceShift >= 3
                )
            {
                log("torque low");

                throttleResult = throttle + step;
                throttleAdj = step;
            }
            else if (
                readyToShift
                && hillClimbActive
                && timeSinceShift >= 3
                )
            {
                log("hill climb");
                throttleResult = throttle + step;
                throttleAdj = step;
            }
            else
            {
                log("do nothing");
                throttleResult = throttle;
                throttleAdj = 0;
            }

            AdjustThrottle(context, throttleAdj);

            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            lastAmps = loco.Amps;
            lastTorque = loco.Torque;
            lastTemperature = loco.Temperature;
        }

        void AdjustThrottle(CruiseControlContext context, float throttleAdj)
        {
            if (throttleAdj == 0) return;

            LocoController loco = context.LocoController;
            loco.Throttle += throttleAdj;
            lastShift = context.Time;
        }

        private void log(string v)
        {
            // logger.Info(v);
        }
    }
}
