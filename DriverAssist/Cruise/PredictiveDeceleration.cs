using System;

namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        static float STEP = 1f / 11f;

        public PredictiveDeceleration()
        {
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            float predictedSpeedKmh = context.Config.BrakingTime * loco.RelativeAccelerationMs * 3.6f;
            float speedKmh = loco.RelativeSpeedKmh + predictedSpeedKmh;
            float brake;

            if (speedKmh > context.DesiredSpeed)
            {
                brake = loco.TrainBrake + STEP;
            }
            else
            {
                brake = loco.TrainBrake - context.Config.BrakeReleaseFactor * loco.TrainBrake;
            }

            brake = Math.Max(brake, context.Config.MinBrake);
            loco.TrainBrake = brake;

            loco.Throttle = 0;
            loco.IndBrake = 0;
        }
    }
}
