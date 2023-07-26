using System;
using System.Collections.Generic;
using DriverAssist.Cruise;
using DV.HUD;
using DV.Simulation.Cars;
using LocoSim.Implementations;
using UnityEngine;

namespace DriverAssist.Implementation
{
    internal class DVTrainCarWrapper : TrainCarWrapper
    {
        private readonly TrainCar loco;

        private BaseControlsOverrider? BaseControls
        {
            get
            {
                return SimController?.controlsOverrider;
            }
        }

        private LocoIndicatorReader? IndicatorReader
        {
            get
            {
                return loco?.loadedInterior?.GetComponent<LocoIndicatorReader>();
            }
        }

        private SimulationFlow? SimFlow
        {
            get
            {
                return loco.GetComponent<SimController>()?.simFlow;
            }
        }

        private Port? TorqueGeneratedPort
        {
            get
            {
                string torqueGeneratedPortId = "traction.TORQUE_IN";
                if (SimFlow != null && torqueGeneratedPortId != null)
                {
                    SimFlow.TryGetPort(torqueGeneratedPortId, out var torqueGeneratedPort);
                    return torqueGeneratedPort;
                }
                return null;
            }
        }

        private SimController? SimController
        {
            get
            {
                return loco.GetComponent<SimController>();
            }
        }

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
                return BaseControls?.Throttle.Value ?? 0;
            }
            set
            {
                BaseControls?.Throttle?.Set(value);
            }
        }

        public float TrainBrake
        {
            get
            {
                return BaseControls?.Brake.Value ?? 0;
            }
            set
            {
                BaseControls?.Brake.Set(value);
            }
        }

        public float IndBrake
        {
            get
            {
                return BaseControls?.IndependentBrake.Value ?? 0;
            }
            set
            {
                BaseControls?.IndependentBrake.Set(value);
            }
        }

        public float GearboxA
        {
            get
            {
                // BaseControlsOverrider overrider = loco.GetComponentInChildren<BaseControlsOverrider>(includeInactive: true);
                InteriorControlsManager controls = loco.interior.GetComponentInChildren<InteriorControlsManager>();
                if (!controls.TryGetControl(InteriorControlsManager.ControlType.GearboxA, out var reference))
                {
                    return 0;
                }
                return reference.controlImplBase.Value;
            }
            // return overrider.bas;

            set
            {
                // BaseControlsOverrider overrider = loco.GetComponentInChildren<BaseControlsOverrider>(includeInactive: true);
                InteriorControlsManager controls = loco.interior.GetComponentInChildren<InteriorControlsManager>();
                if (controls.TryGetControl(InteriorControlsManager.ControlType.GearboxA, out var reference))
                {
                    // controls.TryGetControl(InteriorControlsManager.ControlType.GearboxA, out var reference6)
                    reference.controlImplBase.SetValue(value);
                }
                // return reference.controlImplBase.Value;
            }
        }

        public float GearboxB
        {
            get
            {
                // BaseControlsOverrider overrider = loco.GetComponentInChildren<BaseControlsOverrider>(includeInactive: true);
                InteriorControlsManager controls = loco.interior.GetComponentInChildren<InteriorControlsManager>();
                if (!controls.TryGetControl(InteriorControlsManager.ControlType.GearboxB, out var reference))
                {
                    return 0;
                }
                return reference.controlImplBase.Value;
            }
            // return overrider.bas;

            set
            {
                // BaseControlsOverrider overrider = loco.GetComponentInChildren<BaseControlsOverrider>(includeInactive: true);
                InteriorControlsManager controls = loco.interior.GetComponentInChildren<InteriorControlsManager>();
                if (controls.TryGetControl(InteriorControlsManager.ControlType.GearboxB, out var reference))
                {
                    // controls.TryGetControl(InteriorControlsManager.ControlType.GearboxA, out var reference6)
                    reference.controlImplBase.SetValue(value);
                }
                // return reference.controlImplBase.Value;
            }
        }

        public float Temperature
        {
            get
            {
                if (!IndicatorReader)
                {
                    return 0;
                }

                if (IndicatorReader?.tmTemp)
                    return IndicatorReader?.tmTemp?.Value ?? 0;
                if (IndicatorReader?.oilTemp)
                    return IndicatorReader?.oilTemp?.Value ?? 0;

                return 0;
            }
        }

        public float Reverser
        {
            get
            {
                return BaseControls?.Reverser.Value ?? 0;
            }

            set
            {
                BaseControls?.Reverser.Set(value);
            }
        }


        public float Torque
        {
            get
            {
                return TorqueGeneratedPort?.Value ?? 0f;
            }
        }

        public bool IsElectric
        {
            get
            {
                return LocoType switch
                {
                    DriverAssist.LocoType.DE2 or DriverAssist.LocoType.DE6 => true,
                    _ => false,
                };
            }
        }

        public string TractionMotors
        {
            get
            {
                // int x = locoCar.GetComponent<TractionMotor>().numberOfTractionMotors;
                // SimulationFlow simFlow = loco.GetComponent<SimController>()?.simFlow;
                string maxAmps = "";
                string motors = "";
                // string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.drivingForce.torqueGeneratedPortId;
                if (IsElectric)
                {
                    if (SimFlow != null && SimFlow.TryGetPort("tm.MAX_AMPS", out Port port))
                        maxAmps = "" + port.Value;
                    if (SimFlow != null && SimFlow.TryGetPort("tm.WORKING_TRACTION_MOTORS", out port))
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
                return IndicatorReader?.amps?.Value ?? 0;
            }
        }

        public float Rpm
        {
            get
            {
                return IndicatorReader?.engineRpm?.Value ?? 0;
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

        public float LocoMass
        {
            get
            {
                float mass = 0;

                foreach (TrainCar car in loco.trainset.cars)
                {
                    if (car.IsLoco) mass += car.massController.TotalMass;
                }

                return mass;
            }
        }

        public float CargoMass
        {
            get
            {
                float mass = 0;

                foreach (TrainCar car in loco.trainset.cars)
                {
                    if (!car.IsLoco) mass += car.massController.TotalMass;
                }

                return mass;
            }
        }

        public float WheelRadius
        {
            get
            {
                return loco.carLivery.parentType.wheelRadius;
            }
        }

        public float GearRatio
        {
            get
            {
                switch (LocoType)
                {
                    case DriverAssist.LocoType.DM3:
                        if (SimFlow != null && SimFlow.TryGetPort("transmissionAB.MECHANICAL_GEAR_RATIO", out Port gearRatioPort))
                        {
                            return gearRatioPort.Value;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }
        }

        public bool GearChangeInProgress
        {
            get
            {
                float gearA;
                switch (LocoType)
                {
                    case DriverAssist.LocoType.DM3:
                        if (SimFlow != null && SimFlow.TryGetPort("transmissionA.GEAR_CHANGE_IN_PROGRESS", out Port gearRatioPort))
                        {
                            gearA = gearRatioPort.Value;
                        }
                        else
                            gearA = 0;
                        break;
                    default:
                        gearA = 0;
                        break;
                }

                float gearB;
                switch (LocoType)
                {
                    case DriverAssist.LocoType.DM3:
                        if (SimFlow != null && SimFlow.TryGetPort("transmissionB.GEAR_CHANGE_IN_PROGRESS", out Port gearRatioPort))
                        {
                            gearB = gearRatioPort.Value;
                        }
                        else
                            gearB = 0;
                        break;
                    default:
                        gearB = 0;
                        break;
                }

                return Math.Max(gearA, gearB) > 0;
            }
        }
        public List<string> Ports
        {
            get
            {
                if (SimFlow == null) return new();

                List<string> portstrings = new();
                foreach (SimComponent x in SimFlow.orderedSimComps)
                {
                    List<Port> ports = x.GetAllPorts();
                    foreach (Port p in ports)
                    {
                        portstrings.Add(p.id);
                    }
                }

                return portstrings;
            }
        }

        public int Length { get { return loco.trainset.cars.Count; } }
    }

    public class UnityClock : Clock
    {
        public float Time2 { get { return Time.realtimeSinceStartup; } }
    }
}