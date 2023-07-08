using System;
using System.Collections;
using System.Data;
using BepInEx;
using BepInEx.Logging;
using CommandTerminal;
using DriverAssist.Cruise;
using DV.HUD;
using DV.Simulation.Cars;
using DV.UI.LocoHUD;
using DV.UI.PresetEditors;
using DV.Utils;
using LocoSim.Implementations;
using UnityEngine;
using UnityEngine.UIElements;
// using UnityEngine;

namespace DriverAssist
{
    class CruiseControlTarget
    {
        public float GetSpeed()
        {
            TrainCar locoCar = GetLocomotive();
            float speed = locoCar.GetForwardSpeed() * 3.6f;

            return speed;

        }

        public bool IsLoco()
        {
            return GetLocomotive() != null;

        }
        public void SetThrottle(float throttle)
        {
            TrainCar locoCar = GetLocomotive();
            BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
            obj.Throttle?.Set(throttle);
        }

        public float GetReverser()
        {
            TrainCar locoCar = GetLocomotive();
            BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
            return obj.Reverser.Value;
        }

        public float GetThrottle()
        {
            TrainCar locoCar = GetLocomotive();
            BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;

            return obj.Throttle.Value;
        }

        public float GetTorque()
        {
            float rpm;
            TrainCar locoCar = GetLocomotive();
            SimulationFlow simFlow = locoCar.GetComponent<SimController>()?.simFlow;
            string torqueGeneratedPortId = locoCar.GetComponent<SimController>()?.drivingForce.torqueGeneratedPortId;
            simFlow.TryGetPort(torqueGeneratedPortId, out torqueGeneratedPort);
            rpm = torqueGeneratedPort.Value;
            return rpm;
        }
        private Port torqueGeneratedPort;
        public float GetMass()
        {
            float mass = 0;

            TrainCar locoCar = GetLocomotive();
            foreach (TrainCar car in locoCar.trainset.cars)
            {
                mass += car.massController.TotalMass;
            }

            return mass;
        }

        // public bool TooHot()
        // {
        //     TrainCar locoCar = GetLocomotive();
        //     LocoIndicatorReader locoIndicatorReader = locoCar.loadedInterior?.GetComponent<LocoIndicatorReader>();
        //     return locoIndicatorReader?.tmTemp?.Value >= 100f;
        // }

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

        internal float GetTemperature()
        {
            TrainCar locoCar = GetLocomotive();
            // if (!locoCar.loadedInterior)
            // {
            //     return 0;
            // }
            LocoIndicatorReader locoIndicatorReader = locoCar.loadedInterior?.GetComponent<LocoIndicatorReader>();
            // locoIndicatorReader.
            return locoIndicatorReader?.tmTemp?.Value ?? 0;
        }
    }
}