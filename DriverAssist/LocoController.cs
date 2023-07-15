using System.Collections.Generic;

namespace DriverAssist
{
    public class LocoController
    {
        private Integrator speedIntegratorMs;
        private Integrator heatIntegrator;
        private Integrator ampsIntegrator = new Integrator(60 * 6);
        private Integrator averageAmps = new Integrator(30);
        private float lastSpeedMs;
        private float lastAmps;
        private float lastHeat;
        private TrainCarWrapper loco;
        float lookahead = 1f;
        float temperatureLookAhead = 0.5f;

        private float fixedDeltaTime;

        public LocoController(float fixedDeltaTime)
        {
            loco = NullTrainCarWrapper.Instance;
            this.fixedDeltaTime = fixedDeltaTime;
            int size = (int)(lookahead / fixedDeltaTime);
            speedIntegratorMs = new Integrator(size);

            int size2 = (int)(temperatureLookAhead / fixedDeltaTime);
            heatIntegrator = new Integrator(size2);
            PluginLoggerSingleton.Instance.Info($"{fixedDeltaTime} {size} {size2}");
        }

        public void UpdateLocomotive(TrainCarWrapper newloco)
        {
            // if (!this.loco.IsSameTrainCar(newloco))
            // {
            //     PluginLoggerSingleton.Instance.Info("loco changed");
            // }
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
                loco.Throttle = value;
            }
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

        public float Temperature
        {
            get
            {
                return loco.Temperature;
            }
        }

        public float TemperatureChange
        {
            get
            {
                return heatIntegrator.Integrate() / temperatureLookAhead;
            }
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
                switch (LocoType)
                {
                    case DriverAssist.LocoType.DE2:
                    case DriverAssist.LocoType.DE6:
                        return true;
                    default:
                        return false;
                }
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

        public float AccelerationMs
        {
            get
            {
                return speedIntegratorMs.Integrate() / lookahead;
            }
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

        public string LocoType
        {
            get
            {
                return loco.LocoType;
            }
        }

        public float Mass
        {
            get
            {
                return loco.Mass;
            }
        }

        public bool IsLoco
        {
            get
            {
                return loco.IsLoco;
            }
        }

        // internal void Update()
        // {
        //     OnLocoChange(GetLocomotive());
        // }

        internal void UpdateStats(float deltaTime)
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

        // private TrainCarWrapper GetLocomotive()
        // {
        //     if (!PlayerManager.Car)
        //     {
        //         return new NullTrainCarWrapper();
        //     }
        //     if (!PlayerManager.Car.IsLoco)
        //     {
        //         return new NullTrainCarWrapper();
        //     }

        //     return new DVTrainCarWrapper(PlayerManager.Car);
        // }
    }

    public class LocoType
    {
        public const string DE2 = "LocoShunter";
        public const string DH4 = "LocoDH4";
        public const string DE6 = "LocoDiesel";
        public const string DM3 = "LocoDM3";
        public const string STEAM = "LocoSteamHeavy";
    }

    internal class Integrator
    {
        private List<Node> nodes;
        private int index;
        private int size;

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
                sum += node.value;
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
                sum += node.value;
            }

            return sum;
        }

        internal class Node
        {
            public float value;
            public float time;

            public Node(float value, float time)
            {
                this.value = value;
                this.time = time;
            }
        }
    }

}
