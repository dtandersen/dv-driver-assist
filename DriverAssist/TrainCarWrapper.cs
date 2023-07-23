using System.Collections.Generic;

namespace DriverAssist
{
    public interface TrainCarWrapper
    {
        float SpeedKmh { get; }
        float SpeedMs { get; }
        float Throttle { get; set; }
        float TrainBrake { get; set; }
        float IndBrake { get; set; }
        float GearboxA { get; set; }
        float GearboxB { get; set; }
        float Temperature { get; }
        float Reverser { get; set; }
        float Torque { get; }
        string TractionMotors { get; }
        float Amps { get; }
        float Rpm { get; }
        string LocoType { get; }
        float Mass { get; }
        float LocoMass { get; }
        float CargoMass { get; }
        float WheelRadius { get; }
        float GearRatio { get; }
        List<string> Ports { get; }
        bool GearChangeInProgress { get; }

        bool IsLoco { get; }
        int Length { get; }
    }

    public class NullTrainCarWrapper : TrainCarWrapper
    {
        private static NullTrainCarWrapper instance;

        private NullTrainCarWrapper()
        {
        }

        public static TrainCarWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NullTrainCarWrapper();
                }

                return instance;
            }
        }

        public bool IsLoco { get { return false; } }

        public float SpeedKmh { get; }

        public float SpeedMs { get; }

        public float Throttle { get; set; }
        public float TrainBrake { get; set; }
        public float IndBrake { get; set; }
        public float GearboxA { get; set; }
        public float GearboxB { get; set; }

        public float Temperature { get; }

        public float Reverser { get; set; }

        public float Torque { get; }

        public string TractionMotors { get; }

        public float Amps { get; }

        public float Rpm { get; }

        public string LocoType { get; }

        public float Mass { get; }
        public float LocoMass { get; }
        public float CargoMass { get; }

        public float WheelRadius { get; }
        public float GearRatio { get; }
        public List<string> Ports { get; }
        public bool GearChangeInProgress { get; }

        public int Length { get; }
    }

    public class FakeTrainCarWrapper : TrainCarWrapper
    {
        public bool IsLoco { get { return LocoType != ""; } }

        public float SpeedKmh { get; set; }

        public float SpeedMs { get; }

        public float Throttle { get; set; }
        public float TrainBrake { get; set; }
        public float IndBrake { get; set; }
        public float GearboxA { get; set; }
        public float GearboxB { get; set; }

        public float Temperature { get; set; }

        public float Reverser { get; set; }

        public float Torque { get; set; }

        public string TractionMotors { get; }

        public float Amps { get; set; }

        public float Rpm { get; }

        public string LocoType { get; set; }

        public float Mass { get; }
        public float LocoMass { get; }
        public float CargoMass { get; }
        public float WheelRadius { get; }
        public float GearRatio { get; }
        public List<string> Ports { get; }
        public bool GearChangeInProgress { get; set; }
        public int Length { get; set; }
    }
}
