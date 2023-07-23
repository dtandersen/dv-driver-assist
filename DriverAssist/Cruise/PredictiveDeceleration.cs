using System;

namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        static float STEP = 1f / 11f;
        static float LAP = 0.666f;
        static float RELEASE = 0;

        public PredictiveDeceleration()
        {
        }

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
                if (loco.LocoType != LocoType.DM3)
                {
                    brake = brake + STEP;
                }
                else
                {
                    brake = LAP;
                }
            }
            else
            {
                if (loco.LocoType != LocoType.DM3)
                {
                    brake = brake - context.Config.BrakeReleaseFactor * brake;
                    brake = Math.Max(brake, context.Config.MinBrake);
                }
                else
                {
                    brake = 0;
                }
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
