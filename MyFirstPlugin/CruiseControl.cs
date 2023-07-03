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
// using UnityEngine;

public class CruiseControl
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
    // float lastError = 0;
    // float acc

    public CruiseControl(ManualLogSource logger)
    {
        target = new CruiseControlTarget();
        DesiredSpeed = 0;
        DesiredTorque = 25000;
        this.logger = logger;
        lastThrottle = Time.realtimeSinceStartup;
        torquePid = new Pid(DesiredSpeed, 10000, 0, 0);
        torquePid.Bias = 2500;
        float ku = .025f;
        float tu = 2;
        float kp = .01f;
        // float kp = .005f;
        float ki = .0f;
        // throttlePid = new Pid(0, kp, 0.125f * kp * tu, kp / (0.5f * tu));
        throttlePid = new Pid(0, kp, 0, ki);
        // throttlePid.MaxInt = 100f;
        throttlePid.Bias = 50f;
        // throttlePid.MinInt = -throttlePid.MaxInt;
    }

    public void Tick()
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