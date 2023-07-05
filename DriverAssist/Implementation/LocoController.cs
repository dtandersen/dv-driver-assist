using DV.HUD;
using DV.Simulation.Cars;
using LocoSim.Implementations;

namespace DriverAssist.Implementation
{
    class PlayerLocoController : LocoController
    {
        public float Speed
        {
            get
            {
                TrainCar locoCar = GetLocomotive();
                float speed = locoCar.GetForwardSpeed() * 3.6f;
                return speed;
            }
        }

        public float PositiveSpeed
        {
            get
            {
                if (Reverser > 0)
                    return Speed;
                else
                    return -Speed;
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
                return locoIndicatorReader.tmTemp.Value;
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

        AccelerationMonitor monitor = new AccelerationMonitor();
        float lastSpeed;
        internal void UpdateAcceleration(float deltaTime)
        {
            float a = Speed / 3.6f - lastSpeed / 3.6f;
            monitor.Add(a, deltaTime);
            lastSpeed = Speed;
        }

        public float Acceleration
        {
            get
            {
                return monitor.Sum();
            }
        }

        public float Amps
        {
            get
            {
                TrainCar loco = GetLocomotive();
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader.amps.Value;
            }
        }

        public float Rpm
        {
            get
            {
                TrainCar loco = GetLocomotive();
                LocoIndicatorReader locoIndicatorReader = loco.loadedInterior?.GetComponent<LocoIndicatorReader>();
                return locoIndicatorReader.engineRpm.Value;
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