namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        TrainBrakeDecel automatic = new TrainBrakeDecel();
        ManualLapDecel manualLap = new ManualLapDecel();

        public PredictiveDeceleration()
        {
            automatic = new TrainBrakeDecel();
            manualLap = new ManualLapDecel();
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;

            if (loco.LocoType == LocoType.DM3 && loco.Length > 1)
            {
                manualLap.Tick(context);
            }
            else
            {
                automatic.Tick(context);
            }
        }
    }
}
