using System;
using UnityEngine;

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
        float lastShift;

        public PredictiveAcceleration()
        {
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
            float predictedAmps = loco.Amps + ampDelta * 2f;
            float acceleration = loco.RelativeAccelerationMs;
            float maxamps;
            float minTorque;
            float amps = loco.Amps; //.AverageAmps + 0.05f * loco.AmpsRoc;
            bool ampsdecreased = amps <= lastAmps;
            float maxtemp;
            bool overDriveEnabled = true;
            float minAmps;
            float OVERDRIVE_ACCELL = 0.05f;
            bool hillClimbActive = overDriveEnabled && loco.AccelerationMs < OVERDRIVE_ACCELL;
            float projectedTemperature = loco.Temperature + loco.TemperatureChange;
            // bool tempDecreasing = loco.TemperatureChange < 0;
            // float timeSinceUpshift = Time.realtimeSinceStartup - lastUpshift;
            float timeSinceShift = Time.realtimeSinceStartup - lastShift;
            bool tempDecreasing = loco.Temperature < lastTemperature;
            minTorque = context.Config.MinTorque;
            float operatingTemp = context.Config.MaxTemperature;
            float dangerTemp = context.Config.OverdriveTemperature;
            bool readyToShift =
                torque < minTorque
                && (torque < 10000 || torque <= lastTorque)
                && loco.RelativeAccelerationMs < 0.25f;
            if (hillClimbActive)
            {
                minTorque = context.Config.MinTorque;
                maxtemp = context.Config.OverdriveTemperature;
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
            float temperatureWeight = 1f;
            if (speed > desiredSpeed)
            {
                throttleResult = 0;
            }
            else if (projectedTemperature > dangerTemp)
            {
                throttleResult = throttle - step;
                lastShift = Time.realtimeSinceStartup;
            }
            // else if (overdriveActive)
            // {
            //     throttleResult = throttle;
            //     // lastShift = Time.realtimeSinceStartup;
            // }
            else if (
                !hillClimbActive
                && loco.RelativeAccelerationMs > 0.1
                && projectedTemperature > operatingTemp
                && !tempDecreasing
                && timeSinceShift > 3)
            {
                throttleResult = throttle - step;
                lastShift = Time.realtimeSinceStartup;
            }
            else if (amps < minAmps && loco.IsElectric)
            {
                throttleResult = throttle + step;
                lastShift = Time.realtimeSinceStartup;
            }
            else if (amps > maxamps)
            {
                throttleResult = throttle - step;
                lastShift = Time.realtimeSinceStartup;
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
                readyToShift
                && projectedTemperature < context.Config.MaxTemperature
                && timeSinceShift > 3)
            {
                throttleResult = throttle + step;
                lastShift = Time.realtimeSinceStartup;
            }
            // mid temp no overdrive
            else if (
                readyToShift
                && loco.RelativeAccelerationMs > .1f
                && projectedTemperature > context.Config.MaxTemperature
                && projectedTemperature < context.Config.OverdriveTemperature
                && loco.TemperatureChange < -0.5f
                && timeSinceShift > 3
                )
            {
                throttleResult = throttle + step;
                lastShift = Time.realtimeSinceStartup;
            }
            // overdrive
            else if (
                readyToShift
                && hillClimbActive
                && projectedTemperature < context.Config.OverdriveTemperature
                && timeSinceShift > 3)
            {
                throttleResult = throttle + step;
                lastShift = Time.realtimeSinceStartup;
            }
            else
            {
                throttleResult = throttle;
            }

            loco.Throttle = throttleResult;
            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            lastAmps = loco.Amps;
            lastTorque = loco.Torque;
            lastTemperature = loco.Temperature;
        }
    }
}
