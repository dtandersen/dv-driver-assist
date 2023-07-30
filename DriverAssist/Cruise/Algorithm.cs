using DriverAssist.ECS;

namespace DriverAssist.Cruise
{
    public interface CruiseControlAlgorithm
    {
        void Tick(CruiseControlContext context);
    }

    public class CruiseControlContext
    {
        public LocoSettings Config { get; }
        public LocoEntity LocoController { get; }
        public float DesiredSpeed { get; set; }
        public float Time { get; set; }

        public CruiseControlContext(LocoSettings config, LocoEntity loco)
        {
            Config = config;
            LocoController = loco;
        }
    }

    public class FakeAccelerator : CruiseControlAlgorithm
    {
        public LocoSettings? Settings { get; internal set; }
        public CruiseControlContext? Context { get; internal set; }

        public void Tick(CruiseControlContext context)
        {
            LocoEntity loco = context.LocoController;
            if (loco.RelativeSpeedKmh < context.DesiredSpeed)
            {
                loco.Throttle += .1f;

                Settings = context.Config;
                this.Context = context;
            }
        }
    }

    public class FakeDecelerator : CruiseControlAlgorithm
    {
        public LocoSettings? Settings { get; internal set; }

        public void Tick(CruiseControlContext context)
        {
            LocoEntity loco = context.LocoController;
            if (loco.RelativeSpeedKmh > context.DesiredSpeed)
            {
                loco.TrainBrake += .1f;
                loco.IndBrake += .1f;

                Settings = context.Config;
            }
        }
    }
}
