namespace DriverAssist.Cruise
{
    public interface CruiseControlAlgorithm
    {
        void Tick(CruiseControlContext context);
    }

    public class CruiseControlContext
    {
        public LocoSettings Config { get; }
        public LocoController LocoController { get; }
        public float DesiredSpeed { get; set; }

        public CruiseControlContext(LocoSettings config, LocoController loco)
        {
            Config = config;
            LocoController = loco;
        }
    }

    public class FakeAccelerator : CruiseControlAlgorithm
    {
        public LocoSettings Settings { get; internal set; }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            if (loco.RelativeSpeedKmh < context.DesiredSpeed)
            {
                loco.Throttle += .1f;

                Settings = context.Config;
            }
        }
    }

    public class FakeDecelerator : CruiseControlAlgorithm
    {
        public LocoSettings Settings { get; internal set; }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            if (loco.RelativeSpeedKmh > context.DesiredSpeed)
            {
                loco.TrainBrake += .1f;
                loco.IndBrake += .1f;

                Settings = context.Config;
            }
        }
    }
}
