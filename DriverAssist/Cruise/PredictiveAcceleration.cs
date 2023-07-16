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
        // float lastUpshift;
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
            float predictedAmps = loco.Amps;// + loco.AmpsRoc;
            float acceleration = loco.RelativeAccelerationMs;
            float maxamps;
            float minTorque;
            float amps = loco.Amps; //.AverageAmps + 0.05f * loco.AmpsRoc;
            bool ampsdecreased = amps <= lastAmps;
            float maxtemp;
            // bool overDriveEnabled = true;
            float minAmps;
            float OVERDRIVE_ACCELL = context.Config.HillClimbAccel;
            bool hillClimbActive = loco.AccelerationMs <= context.Config.HillClimbAccel;
            float projectedTemperature = loco.Temperature + loco.TemperatureChange;
            // bool tempDecreasing = loco.TemperatureChange < 0;
            // float timeSinceUpshift = Time.realtimeSinceStartup - lastUpshift;
            float timeSinceShift = context.Time - lastShift;
            bool tempDecreasing = loco.TemperatureChange < 0;
            // bool tempDecreasing = loco.Temperature < lastTemperature;
            minTorque = context.Config.MinTorque;
            float operatingTemp = context.Config.MaxTemperature;
            float dangerTemp = context.Config.HillClimbTemp;
            // float predictedAmps = 1;
            float throttleAdj = 0;

            // bool maxAccelExceeded =
            //     acceleration > context.Config.MaxAccel
            //     && loco.Throttle > 1.5f * step;

            bool readyToShift =
                torque < minTorque
                && (torque < 10000 || torque <= lastTorque)
                && loco.RelativeAccelerationMs < 0.25f;
            if (hillClimbActive)
            {
                minTorque = context.Config.MinTorque;
                maxtemp = context.Config.HillClimbTemp;
                minAmps = context.Config.MinAmps;
                maxamps = context.Config.MaxAmps;
            }
            else
            {
                minTorque = context.Config.MinTorque;
                maxtemp = context.Config.MaxTemperature;
                minAmps = context.Config.MinAmps;
                maxamps = context.Config.MaxAmps;
            }
            log($"predictedAmps{predictedAmps} maxamps={maxamps} timeSinceShift={timeSinceShift}");
            log($"projectedTemperature={projectedTemperature} dangerTemp={dangerTemp}");
            // float temperatureWeight = 1f;
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
                // lastShift = Time.realtimeSinceStartup;
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
                // lastShift = Time.realtimeSinceStartup;
            }
            else if (
                predictedAmps >= maxamps
                && loco.IsElectric)
            {
                log("dangerous amps");
                throttleResult = throttle - step;
                throttleAdj = -step;

                // lastShift = Time.realtimeSinceStartup;
            }
            else if (
                acceleration >= context.Config.MaxAccel
                && loco.Throttle > step)
            // && timeSinceShift > 3)
            {
                log("accelerating too fast");
                // throttleResult = Math.throttle - step;
                throttleResult = Math.Max(throttle - step, step);
                // lastShift = Time.realtimeSinceStartup;
                throttleAdj = throttleResult - loco.Throttle;
            }
            // else if (overdriveActive)
            // {
            //     throttleResult = throttle;
            //     // lastShift = Time.realtimeSinceStartup;
            // }
            else if (
                // back off throttle if we're cruising and the temperature isn't going down
                // !hillClimbActive
                false &&
                loco.RelativeAccelerationMs > context.Config.CruiseAccel
                && projectedTemperature > operatingTemp
                && !tempDecreasing
                && timeSinceShift > 3)
            {
                throttleResult = throttle - step;
                throttleAdj = -step;
                // lastShift = Time.realtimeSinceStartup;
            }
            else if (false && predictedAmps < minAmps && loco.IsElectric)
            {
                throttleResult = throttle + step;
                throttleAdj = step;
                // lastShift = Time.realtimeSinceStartup;
            }
            // // very low temp
            // else if (
            //     readyToShift
            //     && projectedTemperature < context.Config.MaxTemperature * .9f
            //     && timeSinceShift > 3)
            // {
            //     throttleResult = throttle + step;
            //     lastShift = Time.realtimeSinceStartup;
            // }
            // low temp
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

                // lastShift = Time.realtimeSinceStartup;
            }
            // mid temp no overdrive
            else if (
                // readyToShift
                // && loco.RelativeAccelerationMs > .1f
                // && projectedTemperature > context.Config.MaxTemperature
                // && projectedTemperature < context.Config.HillClimbTemp
                false &&
                loco.TemperatureChange < -0.5f
                && timeSinceShift >= 3
                )
            {
                throttleResult = throttle + step;
                throttleAdj = step;

                // lastShift = Time.realtimeSinceStartup;
            }
            else if (
                // false &&
                torque < context.Config.MinTorque
                && acceleration < context.Config.MaxAccel
                // && loco.RelativeAccelerationMs < .1f
                && projectedTemperature < context.Config.MaxTemperature
                // readyToShift
                // && loco.RelativeAccelerationMs > .1f
                // && projectedTemperature > context.Config.MaxTemperature
                // && projectedTemperature < context.Config.HillClimbTemp
                // loco.TemperatureChange < -0.5f
                && timeSinceShift >= 3
                )
            {
                log("torque low");

                throttleResult = throttle + step;
                throttleAdj = step;

                // lastShift = Time.realtimeSinceStartup;
            }
            // overdrive
            else if (
                readyToShift
                && hillClimbActive
                // && projectedTemperature < context.Config.HillClimbTemp
                && timeSinceShift >= 3
                )
            {
                log("hill climb");
                throttleResult = throttle + step;
                throttleAdj = step;
                // lastShift = Time.realtimeSinceStartup;
            }
            else
            {
                log("do nothing");
                throttleResult = throttle;
                throttleAdj = 0;
            }

            // loco.Throttle = throttleResult;
            // throttleAdj = throttleResult - loco.Throttle;
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
            logger.Info(v);
        }
    }
}
