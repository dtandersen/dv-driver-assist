using System;

namespace DriverAssist.Cruise
{
    public class TrainBrakeDecel : CruiseControlAlgorithm
    {
        public const float STEP = 1f / 11f;

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            float predictedSpeedKmh = context.Config.BrakingTime * loco.RelativeAccelerationMs * 3.6f;
            float speedKmh = loco.RelativeSpeedKmh + predictedSpeedKmh;
            float brake;

            if (loco.Length == 1) brake = loco.IndBrake;
            else brake = loco.TrainBrake;

            if (speedKmh > context.DesiredSpeed)
            {
                brake += STEP;
            }
            else
            {
                brake -= context.Config.BrakeReleaseFactor * brake;
                brake = Math.Max(brake, context.Config.MinBrake);
            }

            if (loco.Length == 1)
            {
                loco.TrainBrake = 0;
                loco.IndBrake = brake;
            }
            else
            {
                loco.TrainBrake = brake;
                loco.IndBrake = 0;
            }

            loco.Throttle = 0;
        }
    }
}