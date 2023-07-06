namespace DriverAssist.Cruise
{
    public interface CruiseControlAlgorithm
    {
        float DesiredSpeed { get; set; }

        void Tick(LocoController loco);
    }
}
