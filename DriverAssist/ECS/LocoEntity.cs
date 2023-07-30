using System.Collections.Generic;
using DriverAssist.Cruise;

namespace DriverAssist.ECS
{
    public class LocoEntity
    {
        public readonly LocoComponents Components;

        private readonly Integrator speedIntegratorMs;
        private readonly Integrator heatIntegrator;
        private readonly Integrator ampsIntegrator = new(60 * 6);
        private readonly Integrator averageAmps = new(30);
        private float lastSpeedMs;
        private float lastAmps;
        private float lastHeat;
        private TrainCarWrapper loco;
        private readonly float lookahead = 1f;
        private readonly float temperatureLookAhead = 1f;
        // https://discord.com/channels/332511223536943105/332511223536943105/1129517819416018974
        private readonly List<float[]> gearBox = new() {
            new float[] {0f,   0f},   // 1 = 1-1
            new float[] {0,    0.5f}, // 2 = 1-2
            new float[] {0.5f, 0},    // 3 = 2-1
            new float[] {0.5f, 0.5f}, // 4 = 2-2
            new float[] {1,    0},    // 5 = 3-1
            new float[] {0,    1},    // 6 = 1-3
            new float[] {1,    0.5f}, // 7 = 3-2
            new float[] {0.5f, 1},    // 8 = 2-3
            new float[] {1,    1},    // 9 = 3-3
        };
        private readonly Logger logger;

        public LocoEntity(float fixedDeltaTime)
        {
            logger = LogFactory.GetLogger("LocoController");
            Components = new();
            loco = NullTrainCarWrapper.Instance;
            int size = (int)(lookahead / fixedDeltaTime);
            speedIntegratorMs = new Integrator(size);

            int size2 = (int)(temperatureLookAhead / fixedDeltaTime);
            heatIntegrator = new Integrator(size2);

            int size3 = (int)(lookahead / fixedDeltaTime);
            ampsIntegrator = new Integrator(size3);

            logger.Info($"{fixedDeltaTime} {size} {size2}");
        }

        public void UpdateLocomotive(TrainCarWrapper newloco)
        {
            this.loco = newloco;
        }

        public float SpeedKmh
        {
            get
            {
                return loco.SpeedKmh;
            }
        }

        public float SpeedMs
        {
            get
            {
                return loco.SpeedMs;
            }
        }

        public float RelativeSpeedMs
        {
            get
            {
                if (IsForward) return loco.SpeedMs;
                else return -loco.SpeedMs;
            }
        }

        public float RelativeSpeedKmh
        {
            get
            {
                if (IsForward)
                    return SpeedKmh;
                else
                    return -SpeedKmh;
            }
        }

        public float Throttle
        {
            get
            {
                return loco.Throttle;
            }
            set
            {
                if (GearShiftInProgress) return;

                loco.Throttle = value;
            }
        }

        internal void ZeroThrottle()
        {
            loco.Throttle = 0;
        }

        public float TrainBrake
        {
            get
            {
                return loco.TrainBrake;
            }
            set
            {
                loco.TrainBrake = value;
            }
        }

        public float IndBrake
        {
            get
            {
                return loco.IndBrake;
            }
            set
            {
                loco.IndBrake = value;
            }
        }

        public int Gear
        {
            get
            {
                if (!IsMechanical) return -1;

                for (int i = 0; i < gearBox.Count; i++)
                {
                    float[] gear = gearBox[i];
                    if (gear[0] == loco.GearboxA && gear[1] == loco.GearboxB)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                logger.Info($"{IsMechanical}");
                if (IsMechanical)
                {
                    float[] gear = gearBox[value];
                    loco.GearboxA = gear[0];
                    loco.GearboxB = gear[1];
                    logger.Info($"GearboxA={loco.GearboxA} GearboxB={loco.GearboxB}");
                }
            }
        }

        public float Temperature
        {
            get
            {
                return loco.Temperature;
            }
        }

        public virtual float TemperatureChange
        {
            get
            {
                return heatIntegrator.Integrate() / temperatureLookAhead;
            }

            set { }
        }

        public float Reverser
        {
            get
            {
                return loco.Reverser;
            }

            set
            {
                loco.Reverser = value;
            }
        }

        public float Torque
        {
            get
            {
                return loco.Torque;
            }
        }

        public float RelativeTorque
        {
            get
            {
                if (IsForward) return Torque;
                else return -Torque;
            }
        }

        public string TractionMotors
        {
            get
            {
                return loco.TractionMotors;
            }
        }

        public bool IsElectric
        {
            get
            {
                return Type switch
                {
                    LocoType.DE2 or LocoType.DE6 => true,
                    _ => false,
                };
            }
        }

        public bool IsForward
        {
            get
            {
                return Reverser >= 0.5f;
            }
        }

        public bool IsReversing
        {
            get
            {
                return !IsForward;
            }
        }

        public virtual float AccelerationMs
        {
            get
            {
                return Components.LocoStats.AccelerationMs2;
            }

            set { }
        }

        public float RelativeAccelerationMs
        {
            get
            {
                if (IsForward)
                    return AccelerationMs;
                else
                    return -AccelerationMs;
            }
        }

        public float Amps
        {
            get
            {
                return loco.Amps;
            }
        }

        public float AmpsRoc
        {
            get
            {
                return ampsIntegrator.Integrate();
            }
        }

        public float AverageAmps
        {
            get
            {
                return averageAmps.Average();
            }
        }

        public float Rpm
        {
            get
            {
                return loco.Rpm;
            }
        }

        public string Type
        {
            get
            {
                return loco.Type;
            }
        }

        public float Mass
        {
            get
            {
                return loco.Mass;
            }
        }

        public float LocoMass
        {
            get
            {
                return loco.LocoMass;
            }
        }

        public float CargoMass
        {
            get
            {
                return loco.CargoMass;
            }
        }

        public bool IsLoco
        {
            get
            {
                return loco.IsLoco;
            }
        }

        public bool IsMechanical
        {
            get
            {
                return loco.Type == LocoType.DM3;
            }
        }

        public float GearRatio { get { return loco.GearRatio; } }

        public List<string> Ports { get { return loco.Ports; } }

        public float WheelRadius { get { return loco.WheelRadius; } }

        public bool GearShiftInProgress { get { return loco.GearChangeInProgress; } }

        public int Length { get { return loco.Length; } }

        public bool IsWheelsSlipping { get { return loco.IsWheelSlipping; } }

        public void UpdateStats(float deltaTime)
        {
            if (!loco.IsLoco)
            {
                return;
            }

            float speedMs = SpeedMs;
            float accelerationMs = speedMs - lastSpeedMs;
            speedIntegratorMs.Add(accelerationMs, deltaTime);

            float amps = Amps;
            float ampdelta = amps - lastAmps;
            ampsIntegrator.Add(ampdelta, deltaTime);

            averageAmps.Add(amps, deltaTime);

            float heat = loco.Temperature;
            float deltaHeat = heat - lastHeat;
            heatIntegrator.Add(deltaHeat, deltaTime);

            lastHeat = heat;
            lastAmps = amps;
            lastSpeedMs = speedMs;
        }

        internal void Upshift()
        {
            ChangeGear(Gear + 1);
        }

        internal void Downshift()
        {
            ChangeGear(Gear - 1);
        }

        public void ChangeGear(int requestedGear)
        {
            if (loco.Type != LocoType.DM3) return;

            if (requestedGear < 0) return;
            if (requestedGear >= gearBox.Count) return;
            if (requestedGear == Gear) return;
            if (Components.GearChangeRequest.HasValue) return;

            GearChangeRequest gearChangeRequest = new()
            {
                RequestedGear = requestedGear
            };
            if (loco.Throttle > 0)
            {
                gearChangeRequest.RestoreThrottle = loco.Throttle;
            }
            Components.GearChangeRequest = gearChangeRequest;
            logger.Info($"Requesting gear change RequestedGear={gearChangeRequest.RequestedGear} RestoreThrottle={gearChangeRequest.RestoreThrottle ?? -1}");
        }
    }

    public class LocoType
    {
        public const string DE2 = "LocoShunter";
        public const string DH4 = "LocoDH4";
        public const string DE6 = "LocoDiesel";
        public const string DM3 = "LocoDM3";
        public const string STEAM = "LocoSteamHeavy";
    }

    // public class LocoStats
    // {
    //     private static readonly LocoStats DE2 = new LocoStats("LocoShunter", true);
    //     private static readonly LocoStats DH4 = new LocoStats("LocoDH4", true);
    //     private static readonly LocoStats DE6 = new LocoStats("LocoDiesel", true);
    //     private static readonly LocoStats DM3 = new LocoStats("LocoDM3", false);
    //     private static readonly LocoStats STEAM = new LocoStats("LocoSteamHeavy", true);

    //     public string Id { get; }
    //     public bool SelfLappingTrainBrake { get; }

    //     public LocoStats(string id, bool selfLappingTrainBrake)
    //     {
    //         Id = id;
    //         SelfLappingTrainBrake = selfLappingTrainBrake;
    //     }
    // }

    internal class Integrator
    {
        private readonly List<Node> nodes;
        private int index;
        private readonly int size;

        internal Integrator(int size = 60)
        {
            this.size = size;
            this.nodes = new List<Node>(size);
            this.index = 0;
        }

        internal void Add(float value, float time)
        {
            if (nodes.Count < size)
            {
                nodes.Add(new Node(value, time));
            }
            else
            {
                nodes[index % nodes.Count] = new Node(value, time);
                index++;
            }
        }

        internal float Average()
        {
            float sum = 0;

            foreach (Node node in nodes)
            {
                sum += node.Value;
            }

            return sum / (float)nodes.Count;
        }

        internal float Integrate()
        {
            // float v = 0;
            // float t = 0;

            // foreach (Node node in nodes)
            // {
            //     v += node.value;
            //     t += node.time;
            // }

            // return v * t;
            // float v = 0;
            float sum = 0;

            foreach (Node node in nodes)
            {
                sum += node.Value;
            }

            return sum;
        }

        internal class Node
        {
            public float Value;
            public float Time;

            public Node(float value, float time)
            {
                this.Value = value;
                this.Time = time;
            }
        }
    }

    internal class RollingSample
    {
        private readonly List<float> values;
        private int index;
        private readonly int size;

        internal RollingSample(int size = 60)
        {
            this.size = size;
            this.values = new List<float>(size);
            this.index = 0;
        }

        internal void Add(float value)
        {
            if (values.Count < size)
            {
                values.Add(value);
            }
            else
            {
                values[index % size] = value;
                index++;
            }
        }

        // internal float Average()
        // {
        //     float sum = 0;

        //     foreach (Node node in nodes)
        //     {
        //         sum += node.value;
        //     }

        //     return sum / (float)nodes.Count;
        // }

        internal float Sum()
        {
            // float v = 0;
            // float t = 0;

            // foreach (Node node in nodes)
            // {
            //     v += node.value;
            //     t += node.time;
            // }

            // return v * t;
            // float v = 0;
            float sum = 0;

            foreach (float value in values)
            {
                sum += value;
            }

            return sum;
        }

        // internal class Node
        // {
        //     public float value;
        //     public float time;

        //     public Node(float value, float time)
        //     {
        //         this.value = value;
        //         this.time = time;
        //     }
        // }
    }

    public struct LocoStats
    {
        public float AccelerationMs2;
        public float SpeedMs;
    }

    public struct GearChangeRequest
    {
        public int RequestedGear;
        public float? RestoreThrottle;
    }

    public class LocoComponents
    {
        public GearChangeRequest? GearChangeRequest { get; set; }
        public LocoStats LocoStats { get; set; }
        public float LocoStatsCooldown { get; set; }
        public LocoSettings? LocoSettings { get; set; }
    }
}
