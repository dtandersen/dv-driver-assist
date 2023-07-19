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
            // float throttleResult;
            float predictedAmps = loco.Amps;
            float acceleration = loco.RelativeAccelerationMs;
            float maxamps = context.Config.MaxAmps;
            float minTorque = context.Config.MinTorque;
            float amps = loco.Amps;
            float projectedTemperature = loco.Temperature + loco.TemperatureChange;
            float timeSinceShift = context.Time - lastShift;
            float operatingTemp = context.Config.MaxTemperature;
            float dangerTemp = context.Config.HillClimbTemp;
            // float throttleAdj = 0;

            bool ampsdecreased = amps <= lastAmps;
            bool hillClimbActive = loco.AccelerationMs <= context.Config.HillClimbAccel;
            bool tempDecreasing = loco.TemperatureChange < 0;

            bool readyToShift =
                (torque < minTorque || torque <= lastTorque)
                && loco.RelativeAccelerationMs < context.Config.MaxAccel;

            log($"predictedAmps{predictedAmps} maxamps={maxamps} timeSinceShift={timeSinceShift}");
            log($"projectedTemperature={projectedTemperature} dangerTemp={dangerTemp}");

            if (speed > desiredSpeed)
            {
                log("Reached speed");
                AdjustThrottle(context, -loco.Throttle);
            }
            else if (projectedTemperature >= dangerTemp)
            {
                log("Temperature exceeds danger limit");
                AdjustThrottle(context, -step);
            }
            else if (
                projectedTemperature >= context.Config.MaxTemperature
                && !tempDecreasing
                && !hillClimbActive
                && timeSinceShift >= 3)
            {
                log("Temperature exceeds cruising limit");
                AdjustThrottle(context, -step);
            }
            else if (
                predictedAmps >= maxamps
                && loco.IsElectric)
            {
                log("Amps exceed limit");
                AdjustThrottle(context, -step);
            }
            else if (
                acceleration >= context.Config.MaxAccel
                && loco.Throttle > step)
            {
                log("Acceleration limit reached");
                float desiredThrottle = Math.Max(throttle - step, step);
                AdjustThrottle(context, desiredThrottle - loco.Throttle);
            }
            // else if (
            //     acceleration < context.Config.MaxAccel
            //     && torque < context.Config.MinTorque
            //     && projectedTemperature < context.Config.MaxTemperature
            //     && tempDecreasing
            //     && timeSinceShift >= 3
            //     )
            // {
            //     log($"Low acceleration");
            //     log($"acceleration={acceleration} MaxAccel={context.Config.MaxAccel}");
            //     AdjustThrottle(context, step);
            // }
            else if (
                torque < context.Config.MinTorque
                && acceleration < context.Config.MaxAccel
                && projectedTemperature < context.Config.MaxTemperature
                && (tempDecreasing || projectedTemperature <= context.Config.MaxTemperature - 5)
                && timeSinceShift >= 3
                )
            {
                log("Low torque");
                AdjustThrottle(context, step);
            }
            else if (
                readyToShift
                && hillClimbActive
                && timeSinceShift >= 3
                )
            {
                log("Hill climbing");
                AdjustThrottle(context, step);
            }
            else
            {
                log("do nothing");
            }

            if (loco.Rpm > 800)
            {
                loco.RequestedGear++;
            }
            if (loco.Rpm < 600)
            {
                loco.RequestedGear--;
            }

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
