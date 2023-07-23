using System;

namespace DriverAssist.Cruise
{
    public class ManualLapDecel : CruiseControlAlgorithm
    {
        public const float LAP = 2f / 3f;
        public const float RELEASE = 0;

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            float predictedSpeedKmh = context.Config.BrakingTime * loco.RelativeAccelerationMs * 3.6f;
            float speedKmh = loco.RelativeSpeedKmh + predictedSpeedKmh;
            float brake;

            if (speedKmh > context.DesiredSpeed)
            {
                brake = LAP;
            }
            else
            {
                brake = RELEASE;
            }

            loco.TrainBrake = brake;
            loco.IndBrake = 0;
            loco.Throttle = 0;
        }
    }
}
