using System;

namespace DriverAssist.Cruise
{
    public class PredictiveAcceleration : CruiseControlAlgorithm
    {
        // float lastAmps = 0;
        float lastTorque = 0;
        // float lastTemperature = 0;
        float lastRpm = 0;
        const float STEP = 1f / 11f;
        // bool cooling = false;
        public float LastThrottleChange;
        public float LastShift;
        readonly Logger logger;

        public PredictiveAcceleration()
        {
            logger = LogFactory.GetLogger("PredictiveAcceleration");
        }

        public void Tick(CruiseControlContext context)
        {
            // PluginLoggerSingleton.ThreadInstance.Value.Info("test");
            LocoController loco = context.LocoController;

            // float reverser;// = loco.Reverser;
            float speed = loco.RelativeSpeedKmh;
            float desiredSpeed = context.DesiredSpeed;
            float throttle = loco.Throttle;
            float torque = loco.RelativeTorque;
            // float temperature = loco.Temperature;
            // float ampDelta = loco.Amps - lastAmps;
            // float throttleResult;
            float predictedAmps = loco.Amps;
            float acceleration = loco.RelativeAccelerationMs;
            float maxamps = context.Config.MaxAmps;
            float minTorque = context.Config.MinTorque;
            // float amps = loco.Amps;
            float projectedTemperature = loco.Temperature + loco.TemperatureChange;
            float timeSinceThrottle = context.Time - LastThrottleChange;
            // float operatingTemp = context.Config.MaxTemperature;
            float dangerTemp = context.Config.HillClimbTemp;
            // float throttleAdj = 0;

            // bool ampsdecreased = amps <= lastAmps;
            bool hillClimbActive = loco.AccelerationMs <= context.Config.HillClimbAccel;
            bool tempDecreasing = loco.TemperatureChange < 0;

            bool readyToThrottle =
                (torque < minTorque || torque <= lastTorque)
                && loco.RelativeAccelerationMs < context.Config.MaxAccel;

            Log($"predictedAmps{predictedAmps} maxamps={maxamps} timeSinceShift={timeSinceThrottle}");
            Log($"projectedTemperature={projectedTemperature} dangerTemp={dangerTemp}");

            if (speed > desiredSpeed)
            {
                Log("Reached speed");
                AdjustThrottle(context, -loco.Throttle);
            }
            else if (projectedTemperature >= dangerTemp)
            {
                Log("Temperature exceeds danger limit");
                AdjustThrottle(context, -STEP);
            }
            else if (
                projectedTemperature >= context.Config.OperatingTemp
                && !tempDecreasing
                && !hillClimbActive
                && timeSinceThrottle >= 3)
            {
                Log("Temperature exceeds cruising limit");
                AdjustThrottle(context, -STEP);
            }
            else if (
                predictedAmps >= maxamps
                && loco.IsElectric)
            {
                Log("Amps exceed limit");
                AdjustThrottle(context, -STEP);
            }
            else if (
                loco.IsWheelsSlipping)
            {
                Log("Wheels are slipping");
                AdjustThrottle(context, -STEP);
            }
            else if (
                acceleration >= context.Config.MaxAccel
                && loco.Throttle > STEP)
            {
                Log("Acceleration limit reached");
                float desiredThrottle = Math.Max(throttle - STEP, STEP);
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
                && projectedTemperature < context.Config.OperatingTemp
                && (tempDecreasing || projectedTemperature <= context.Config.OperatingTemp - 5)
                && timeSinceThrottle >= 3
                )
            {
                Log("Low torque");
                AdjustThrottle(context, STEP);
            }
            else if (
                readyToThrottle
                && hillClimbActive
                && timeSinceThrottle >= 3
                )
            {
                Log("Hill climbing");
                AdjustThrottle(context, STEP);
            }
            else
            {
                Log("do nothing");
            }

            if (loco.Rpm > 800)
            {
                loco.ChangeGear(loco.Gear + 1);
            }
            if (loco.Rpm < 600 && !(loco.Rpm > lastRpm))
            {
                loco.ChangeGear(loco.Gear - 1);
            }

            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            // lastAmps = loco.Amps;
            lastTorque = loco.Torque;
            // lastTemperature = loco.Temperature;
            lastRpm = loco.Rpm;
        }

        void AdjustThrottle(CruiseControlContext context, float throttleAdj)
        {
            if (throttleAdj == 0) return;

            LocoController loco = context.LocoController;
            loco.Throttle += throttleAdj;
            LastThrottleChange = context.Time;
        }

        private void Log(string v)
        {
            if (Math.Sin(0) == 1)
            {
                logger.Info(v);
            }
        }
    }
}
