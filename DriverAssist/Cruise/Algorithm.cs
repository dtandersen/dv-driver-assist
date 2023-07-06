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
}
