using System.Diagnostics;
using DV.HUD;
using DV.Simulation.Cars;
using LocoSim.Implementations;

namespace CruiseControlPlugin
{
    public interface LocoController
    {
        float Throttle { get; set; }
        float TrainBrake { get; set; }
        float IndBrake { get; set; }
        float Speed { get; }
        float Temperature { get; }
        float Torque { get; }
        float Reverser { get; }
    }

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

    interface PluginLogger
    {
        public void Info(string message);
    }

    class LoggerSingleton
    {
        public static PluginLogger Instance = new NullLogger();
    }

    internal class NullLogger : PluginLogger
    {
        public void Info(string message)
        {
        }
    }
}
