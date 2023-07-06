using System;

namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        public float DesiredSpeed { get; set; }

        public PredictiveDeceleration()
        {
        }

        public void Tick(LocoController loco)
        {
            loco.Throttle = 0;
            loco.IndBrake = 0;
            loco.TrainBrake += .1f;
        }
    }
}