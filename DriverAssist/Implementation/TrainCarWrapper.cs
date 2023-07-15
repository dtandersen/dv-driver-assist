using DV.HUD;
using DV.Simulation.Cars;
using LocoSim.Implementations;

namespace DriverAssist.Implementation
{
    internal class DVTrainCarWrapper : TrainCarWrapper
    {
        private TrainCar loco;

        public DVTrainCarWrapper(TrainCar car)
        {
            this.loco = car;
        }

        public bool IsLoco { get { return true; } }

        public float SpeedKmh
        {
            get
            {
                float speed = loco.GetForwardSpeed() * 3.6f;
                return speed;
            }
        }

        public float SpeedMs
        {
            get
            {
                float speed = loco.GetForwardSpeed();
                return speed;
            }
        }

        public float Throttle
        {
            get
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                return obj.Throttle.Value;
            }
            set
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                obj.Throttle?.Set(value);
            }
        }

        public float TrainBrake
        {
            get
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                return obj.Brake.Value;
            }
            set
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                obj.Brake.Set(value);
            }
        }

        public float IndBrake
        {
            get
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                return obj.IndependentBrake.Value;
            }
            set
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                obj.IndependentBrake.Set(value);
            }
        }

        public float Temperature
        {
            get
            {
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
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
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                return obj.Reverser.Value;
            }

            set
            {
                BaseControlsOverrider obj = loco.GetComponent<SimController>()?.controlsOverrider;
                obj.Reverser.Set(value);
            }
        }

        private Port torqueGeneratedPort;

        public float Torque
        {
            get
            {
                float torque;
                SimulationFlow simFlow = loco.GetComponent<SimController>()?.simFlow;
                string torqueGeneratedPortId = loco.GetComponent<SimController>()?.drivingForce.torqueGeneratedPortId;
                simFlow.TryGetPort(torqueGeneratedPortId, out torqueGeneratedPort);
                torque = torqueGeneratedPort.Value;
                return torque;
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

        public string TractionMotors
        {
            get
            {
                // int x = locoCar.GetComponent<TractionMotor>().numberOfTractionMotors;
                SimulationFlow simFlow = loco.GetComponent<SimController>()?.simFlow;
                Port port;
                string maxAmps = "";
                string motors = "";
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

        public float Amps
        {
            get
            {
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader?.amps?.Value ?? 0;
            }
        }

        public float Rpm
        {
            get
            {
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader?.engineRpm?.Value ?? 0;
            }
        }

        public string LocoType
        {
            get
            {
                return loco.carType.ToString();
            }
        }

        public float Mass
        {
            get
            {
                float mass = 0;

                foreach (TrainCar car in loco.trainset.cars)
                {
                    mass += car.massController.TotalMass;
                }

                return mass;
            }
        }

        // public bool IsSameTrainCar(TrainCarWrapper newloco)
        // {
        //     if (newloco.GetType() != typeof(DVTrainCarWrapper)) return false;

        //     return loco == ((DVTrainCarWrapper)newloco).loco;
        // }

        public static bool IsSameTrainCar2(TrainCarWrapper currentLoco, TrainCar trainCar)
        {
            if (currentLoco.GetType() != typeof(DVTrainCarWrapper)) return false;

            return ((DVTrainCarWrapper)currentLoco).loco == trainCar;
        }
    }
}
