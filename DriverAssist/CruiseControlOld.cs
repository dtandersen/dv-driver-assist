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
    public class CruiseControlOld
    {
        public float DesiredSpeed { get; set; }
        public float DesiredTorque { get; set; }
        public float Force { get; set; }
        public float Power { get; set; }
        public float Hoursepower { get; set; }
        public float Torque { get; set; }
        public float Mass { get; set; }
        public float Acceleration { get; set; }
        public float Speed { get; set; }
        public float Throttle { get; set; }
        public float Temperature { get; set; }

        ManualLogSource logger;
        CruiseControlTarget target;
        bool Enabled { get; set; }
        // float lastThrottle;
        float elapsedTime;
        float dtMax = 1f;
        float lastSpeed = 0;
        // float lastTorque = 0;
        private PredictiveAcceleration accelerate;
        private PredictiveDeceleration decelerate;


        public CruiseControlOld(ManualLogSource logger)
        {
            target = new CruiseControlTarget();
            DesiredSpeed = 0;
            DesiredTorque = 25000;
            this.logger = logger;
            // lastThrottle = Time.realtimeSinceStartup;
            accelerate = new PredictiveAcceleration();
            decelerate = new PredictiveDeceleration();
        }

        public void Tick()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < dtMax)
            {
                return;
            }

            UpdateStats();

            if (!target.IsLoco())
            {
                Enabled = false;
                return;
            }

            if (DesiredSpeed > 0)
            {
                Enabled = true;
            }
            if (!Enabled)
            {
                return;
            }
            if (DesiredSpeed <= 0)
            {
                DesiredSpeed = 0;
                target.SetThrottle(0f);
                Enabled = false;
                return;
            }
            float reverser = target.GetReverser();
            if (reverser <= 0.5f)
            {
                target.SetThrottle(0f);
                Enabled = false;
                return;
            }

            float AccelSp = DesiredSpeed - 5;
            float DecelSp = DesiredSpeed;

            if (Speed < AccelSp)
            {
                accelerate.DesiredSpeed = AccelSp;
                accelerate.DesiredTorque = DesiredTorque;
                accelerate.Tick(null);
            }
            else if (Speed > DecelSp)
            {
                decelerate.DesiredSpeed = DecelSp;
                decelerate.Tick(null);
            }

            elapsedTime = 0;
        }

        private void UpdateStats()
        {
            Speed = target.GetSpeed();
            double accel = (Speed / 3.6f - lastSpeed / 3.6f) * elapsedTime;
            Acceleration = (float)Math.Round(accel, 2);
            Throttle = target.GetThrottle();
            Mass = target.GetMass();
            Power = Mass * 9.8f / 2f * Speed / 3.6f;
            Force = Mass * 9.8f / 2f;
            Hoursepower = Power / 745.7f;
            Torque = target.GetTorque();
            lastSpeed = Speed;
        }
    }

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

        public bool TooHot()
        {
            TrainCar locoCar = GetLocomotive();
            LocoIndicatorReader locoIndicatorReader = locoCar.loadedInterior?.GetComponent<LocoIndicatorReader>();
            return locoIndicatorReader.tmTemp.Value >= 100f;
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

        internal float GetTemperature()
        {
            TrainCar locoCar = GetLocomotive();
            LocoIndicatorReader locoIndicatorReader = locoCar.loadedInterior?.GetComponent<LocoIndicatorReader>();
            return locoIndicatorReader.tmTemp.Value;
        }
    }
}