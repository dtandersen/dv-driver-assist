using DriverAssist.ECS;

namespace DriverAssist.Cruise
{
    public class PredictiveDeceleration : CruiseControlAlgorithm
    {
        readonly TrainBrakeDecel automatic = new();
        readonly ManualLapDecel manualLap = new();

        public PredictiveDeceleration()
        {
            automatic = new TrainBrakeDecel();
            manualLap = new ManualLapDecel();
        }

        public void Tick(CruiseControlContext context)
        {
            LocoEntity loco = context.LocoController;

            if (loco.Type == LocoType.DM3 && loco.Length > 1)
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
