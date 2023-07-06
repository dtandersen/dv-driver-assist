namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        static float STEP = 1f / 11f;
        float lookAhead = 10;

        public PredictiveDeceleration()
        {
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            float speed = loco.RelativeSpeed + lookAhead * loco.RelativeAcceleration;

            if (speed > context.DesiredSpeed)
            {
                loco.TrainBrake += STEP;
            }
            else
            {
                loco.TrainBrake /= 2;
            }

            loco.Throttle = 0;
            loco.IndBrake = 0;
        }
    }
}
