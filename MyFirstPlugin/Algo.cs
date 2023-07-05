using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using CommandTerminal;
using DV.HUD;
using DV.Simulation.Cars;
using DV.UI.LocoHUD;
using DV.UI.PresetEditors;
using DV.Utils;
using LocoSim.Implementations;
using UnityEngine;
using UnityEngine.UIElements;

namespace CruiseControlPlugin.Algorithm
{
    public interface CruiseControlAlgorithm
    {
        float DesiredSpeed { get; set; }

        void Tick(LocoController loco);
    }

    internal class DefaultAccelerationAlgo : CruiseControlAlgorithm
    {
        public float DesiredSpeed { get; set; }
        public float DesiredTorque { get; set; }

        float lastTorque = 0;
        float lastAmps = 0;
        float step = 1f / 11f;
        private DefaultAccelerationAlgo accelerate;
        private DefaultDecelerationAlgo decelerate;

        public DefaultAccelerationAlgo()
        {
            DesiredTorque = 25000;
        }

        public void Tick(LocoController loco)
        {
            // Debug.Log($"DesiredSpeed={DesiredSpeed}");

            float reverser = loco.Reverser;
            float speed = loco.PositiveSpeed;
            float desiredSpeed = DesiredSpeed;
            // if (reverser == 1)
            // {
            //     desiredSpeed = DesiredSpeed;
            // }
            // else
            // {
            //     desiredSpeed = -DesiredSpeed;
            // }
            float throttle = loco.Throttle;
            float torque = loco.Torque;
            float temperature = loco.Temperature;
            float ampDelta = loco.Amps - lastAmps;
            float throttleResult;
            float projectedAmps = loco.Amps + ampDelta * 3f;
            if (speed < 5)
            {
                throttleResult = step;
            }
            else if (speed > desiredSpeed)
            {
                throttleResult = 0;
            }
            else if (loco.Temperature > 100)
            {
                throttleResult = throttle - step;
            }
            else if (projectedAmps > 550)
            {
                throttleResult = throttle - step;
            }
            else if (projectedAmps < 450)
            {
                throttleResult = throttle + step;
            }
            else if (loco.Amps > 600 && !(loco.Amps < lastAmps))
            {
                throttleResult = throttle - step;
            }
            else
            {
                throttleResult = throttle;
            }

            loco.Throttle = throttleResult;
            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            lastTorque = torque;
            lastAmps = loco.Amps;
        }
    }

    internal class DefaultDecelerationAlgo : CruiseControlAlgorithm
    {
        public float DesiredSpeed { get; set; }

        public DefaultDecelerationAlgo()
        {
        }

        public void Tick(LocoController loco)
        {
            loco.Throttle = 0;
            loco.IndBrake = 0;
            loco.TrainBrake += .1f;
        }
    }

    internal class PidAccelerationAlgo : CruiseControlAlgorithm
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
        // float kp = .0025f;
        // float kd = 0f;
        // float ki = .0006f;
        bool Enabled { get; set; }
        float lastThrottle;
        private Pid throttlePid;
        private Pid torquePid;
        float currentTime;
        float dt;
        float dtMax = 1f;
        float lastSpeed = 0;
        float lastTorque = 0;
        private DefaultAccelerationAlgo accelerate;
        private DefaultDecelerationAlgo decelerate;

        public PidAccelerationAlgo()
        {
        }

        public void Tick(LocoController loco)
        {
            // logger.LogInfo("tick");
            currentTime = Time.realtimeSinceStartup;
            dt = currentTime - lastThrottle;
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
            if (dt < dtMax)
            {
                return;
            }
            lastThrottle = Time.realtimeSinceStartup;
            float currentSpeed = target.GetSpeed();
            double accel = (currentSpeed / 3.6f - lastSpeed / 3.6f) * dtMax;
            Speed = currentSpeed;
            Acceleration = (float)Math.Round(accel, 2);
            Throttle = target.GetThrottle();
            Mass = target.GetMass();
            Power = Mass * 9.8f / 2f * Speed / 3.6f;
            Force = Mass * 9.8f / 2f;
            Hoursepower = Power / 745.7f;
            Torque = target.GetTorque();

            torquePid.SetPoint = DesiredSpeed;
            float torqueResult = torquePid.evaluate(currentSpeed);
            torqueResult = Math.Min(torqueResult, DesiredTorque);

            throttlePid.SetPoint = torqueResult;
            float throttleResult = throttlePid.evaluate(Torque) / 100f;
            Temperature = target.GetTemperature();
            // if (throttleResult > 1 || throttleResult < 0)
            // {
            //     throttlePid.Unwind();
            // }
            float step = 0.1f;

            if (Speed > DesiredSpeed)
            {
                throttleResult = 0;
            }
            else if (target.TooHot())
            {
                throttleResult = Throttle - step;
            }
            else if (DesiredTorque > Torque)
            {
                throttleResult = Throttle + step;
            }
            else if (DesiredTorque < Torque && !(Torque < lastTorque))
            {
                throttleResult = Throttle - step;
            }
            else
            {
                throttleResult = Throttle;
            }
            // if (throttleResult > Throttle)
            // {
            //     throttleResult = Throttle + step;
            // }
            // else if (throttleResult < Throttle)
            // {
            //     throttleResult = Throttle - step;
            // }

            // if (Speed < 5)
            // {
            //     throttleResult = (float)Math.Min(.1, throttleResult);
            // }

            target.SetThrottle(throttleResult);
            lastSpeed = currentSpeed;
            lastTorque = Torque;

            logger.LogInfo($"torquePid={torquePid}, throttlePid={throttlePid}");
        }
    }
}