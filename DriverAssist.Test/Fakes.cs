using System.Collections.Generic;
using DriverAssist.Cruise;

namespace DriverAssist
{
    public class FakeCruiseControlConfig : CruiseControlSettings
    {
        public FakeCruiseControlConfig()
        {
            LocoSettings = new Dictionary<string, LocoSettings>();
            Acceleration = "";
            Deceleration = "";
        }

        public int MinTorque { get; set; }
        public int MinAmps { get; }
        public int MaxAmps { get; }
        public int MaxTemperature { get; }
        public int OverdriveTemperature { get; }
        public bool OverdriveEnabled { get; }
        public float Offset { get; set; }
        public float Diff { get; set; }
        public float UpdateInterval { get; set; }

        public string Acceleration { get; set; }
        public string Deceleration { get; set; }
        public Dictionary<string, LocoSettings> LocoSettings { get; }
    }

    public class FakeLocoConfig : LocoSettings
    {
        public int MinTorque { get; set; }
        public int MinAmps { get; set; }
        public int MaxAmps { get; set; }
        public int MaxTemperature { get; set; }
        public int HillClimbTemp { get; set; }
        public bool OverdriveEnabled { get; set; }
        public int BrakingTime { get; set; }
        public float BrakeReleaseFactor { get; set; }
        public float MinBrake { get; set; }
        public float HillClimbAccel { get; set; }
        public float CruiseAccel { get; set; }
        public float MaxAccel { get; set; }
    }

    public class FakeClock : Clock
    {
        public float Time2 { get; set; }
    }
}
