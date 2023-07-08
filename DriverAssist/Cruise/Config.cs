using System.Collections.Generic;

namespace DriverAssist.Cruise
{
    public interface CruiseControlConfig : LocoConfig
    {
        float Offset { get; }
        float Diff { get; }
        float UpdateInterval { get; }
        string Acceleration { get; }
        string Deceleration { get; }
        Dictionary<string, LocoConfig> LocoSettings { get; }
    }

    public interface LocoConfig
    {
        int MinTorque { get; }
        int MinAmps { get; }
        int MaxAmps { get; }
        int MaxTemperature { get; }
        int OverdriveTemperature { get; }
        bool OverdriveEnabled { get; }
    }
}
