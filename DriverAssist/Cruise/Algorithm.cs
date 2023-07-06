namespace DriverAssist.Cruise
{
    public interface CruiseControlAlgorithm
    {
        void Tick(CruiseControlContext context);
    }

    public class CruiseControlContext
    {
        public CruiseControlConfig Config { get; }
        public LocoController LocoController { get; }
        public float DesiredSpeed { get; set; }

        public CruiseControlContext(CruiseControlConfig config, LocoController loco)
        {
            Config = config;
            LocoController = loco;
        }
    }

    public class FakeAccelerator : CruiseControlAlgorithm
    {
        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            if (loco.RelativeSpeed < context.DesiredSpeed)
            {
                loco.Throttle += .1f;
            }
        }
    }

    public class FakeDecelerator : CruiseControlAlgorithm
    {
        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            if (loco.RelativeSpeed > context.DesiredSpeed)
            {
                loco.TrainBrake += .1f;
                loco.IndBrake += .1f;
            }
        }
    }
}
