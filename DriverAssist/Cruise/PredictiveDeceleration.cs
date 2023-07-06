namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        public float DesiredSpeed { get; set; }

        public PredictiveDeceleration()
        {
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            loco.Throttle = 0;
            loco.IndBrake = 0;
            loco.TrainBrake += .1f;
        }
    }
}