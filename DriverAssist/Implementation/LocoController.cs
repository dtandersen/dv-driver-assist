using System;
using DV.HUD;
using DV.Simulation.Cars;
using LocoSim.Implementations;

namespace DriverAssist.Implementation
{
    class PlayerLocoController : LocoController
    {
        private Integrator speedIntegrator = new Integrator();
        private Integrator ampsIntegrator = new Integrator(60 * 6);
        private Integrator averageAmps = new Integrator(30);
        private float lastSpeed;
        private float lastAmps;

        public float SpeedKmh
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                float speed = locoCar.GetForwardSpeed() * 3.6f;
                return speed;
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
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                return obj.Throttle.Value;
            }
            set
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                obj.Throttle?.Set(value);
            }
        }
        public float TrainBrake
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                return obj.Brake.Value;
            }
            set
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                obj.Brake.Set(value);
            }
        }

        public float IndBrake
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                return obj.IndependentBrake.Value;
            }
            set
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                obj.IndependentBrake.Set(value);
            }
        }

        public float Temperature
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                LocoIndicatorReader locoIndicatorReader = locoCar.loadedInterior?.GetComponent<LocoIndicatorReader>();
                if (!locoIndicatorReader)
                {
                    return 0;
                }

                if (locoIndicatorReader?.tmTemp)
                    return locoIndicatorReader?.tmTemp?.Value ?? 0;
                if (locoIndicatorReader?.oilTemp)
                    return locoIndicatorReader?.oilTemp?.Value ?? 0;

                return 0;
            }
        }

        public float Reverser
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                return obj.Reverser.Value;
            }

            set
            {
                TrainCar locoCar = GetLocomotive();
                BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
                obj.Reverser.Set(value);
            }
        }

        private Port torqueGeneratedPort;

        public float Torque
        {
            get
            {
                float torque;
                TrainCar locoCar = GetLocomotive();
                SimulationFlow simFlow = locoCar.GetComponent<SimController>()?.simFlow;
                string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.drivingForce.torqueGeneratedPortId;
                simFlow.TryGetPort(torqueGeneratedPortId, out torqueGeneratedPort);
                torque = torqueGeneratedPort.Value;
                return torque;
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
                TrainCar locoCar = GetLocomotive();
                // int x = locoCar.GetComponent<TractionMotor>().numberOfTractionMotors;
                SimulationFlow simFlow = locoCar.GetComponent<SimController>()?.simFlow;
                Port port;
                String maxAmps = "";
                String motors = "";
                // string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.drivingForce.torqueGeneratedPortId;
                if (IsElectric)
                {
                    if (simFlow.TryGetPort("tm.MAX_AMPS", out port))
                        maxAmps = "" + port.Value;
                    if (simFlow.TryGetPort("tm.WORKING_TRACTION_MOTORS", out port))
                        motors = "" + port.Value;
                    return $"{motors} / {maxAmps}";
                }
                else { return ""; }
            }

            // simFlow.TryGetPort("tm.WORKING_TRACTION_MOTORS", out port);

            // foreach (SimComponent x in simFlow.orderedSimComps)
            // {
            //     List<Port> ports = x.GetAllPorts();
            //     foreach (Port p in ports)
            //     {
            //         PluginLoggerSingleton.Instance.Info($"{x} {p.id}");
            //     }
            // }
            // string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.tr;

            // locoCar.def
            // return "" + port.Value;
            // return torqueGeneratedPortId;
            // }
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

            // simFlow.TryGetPort("tm.WORKING_TRACTION_MOTORS", out port);

            // foreach (SimComponent x in simFlow.orderedSimComps)
            // {
            //     List<Port> ports = x.GetAllPorts();
            //     foreach (Port p in ports)
            //     {
            //         PluginLoggerSingleton.Instance.Info($"{x} {p.id}");
            //     }
            // }
            // string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.tr;

            // locoCar.def
            // return "" + port.Value;
            // return torqueGeneratedPortId;
            // }
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

        internal void UpdateAcceleration(float deltaTime)
        {
            float speed = SpeedKmh;
            float a = speed / 3.6f - lastSpeed / 3.6f;
            speedIntegrator.Add(a, deltaTime);

            float amps = Amps;
            float ampdelta = amps - lastAmps;
            ampsIntegrator.Add(ampdelta, deltaTime);

            averageAmps.Add(amps, deltaTime);

            lastAmps = amps;
            lastSpeed = speed;
        }

        public float AccelerationMs
        {
            get
            {
                return speedIntegrator.Integrate();
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
                TrainCar loco = GetLocomotive();
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader?.amps?.Value ?? 0;
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
                TrainCar loco = GetLocomotive();
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader?.engineRpm?.Value ?? 0;
            }
        }

        public string LocoType
        {
            get
            {
                TrainCar loco = GetLocomotive();
                return loco.carType.ToString();
            }
        }

        public float Mass
        {
            get
            {
                float mass = 0;

                TrainCar locoCar = GetLocomotive();
                foreach (TrainCar car in locoCar.trainset.cars)
                {
                    mass += car.massController.TotalMass;
                }

                return mass;
            }
        }

        public bool IsLoco
        {
            get
            {
                return GetLocomotive() != null;
            }
        }

        private TrainCar GetLocomotive()
        {
            if (!PlayerManager.Car)
            {
                return null;
            }
            if (!PlayerManager.Car.IsLoco)
            {
                return null;
            }
            return PlayerManager.Car;
        }
    }
}