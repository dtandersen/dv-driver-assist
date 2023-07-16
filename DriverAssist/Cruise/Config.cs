using System.Collections.Generic;

namespace DriverAssist.Cruise
{
    public interface CruiseControlSettings
    {
        float Offset { get; }
        float Diff { get; }
        float UpdateInterval { get; }
        string Acceleration { get; }
        string Deceleration { get; }
        Dictionary<string, LocoSettings> LocoSettings { get; }
    }

    public interface LocoSettings
    {
        int MinTorque { get; }
        int MinAmps { get; }
        int MaxAmps { get; }
        int MaxTemperature { get; }
        int HillClimbTemp { get; }
        int BrakingTime { get; }
        float BrakeReleaseFactor { get; }
        float MinBrake { get; }
        float HillClimbAccel { get; }
        float CruiseAccel { get; }
        float MaxAccel { get; }
    }
}
