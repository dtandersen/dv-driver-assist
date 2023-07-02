using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using CommandTerminal;
using DV.HUD;
using DV.Simulation.Cars;
using DV.UI.LocoHUD;
using DV.Utils;
using UnityEngine;
using UnityEngine.UIElements;
// using UnityEngine;

public class CruiseControl
{
    public float SetPoint { get; set; }
    public float Acceleration { get; set; }
    public float Speed { get; set; }
    public float Throttle { get; set; }
    ManualLogSource logger;
    CruiseControlTarget target;
    float kp = .1f;
    float kd = 0f;
    bool Enabled { get; set; }
    float lastThrottle;
    float currentTime;
    float dt;
    float dtMax = 1f;
    float lastSpeed = 0;
    float lastError = 0;
    // float acc

    public CruiseControl(ManualLogSource logger)
    {
        target = new CruiseControlTarget();
        SetPoint = 0;
        this.logger = logger;
        lastThrottle = Time.realtimeSinceStartup;
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

        if (SetPoint > 0)
        {
            Enabled = true;
        }
        if (!Enabled)
        {
            return;
        }
        if (SetPoint <= 0)
        {
            SetPoint = 0;
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
        Speed = currentSpeed;
        double a = (currentSpeed / 3.6f - lastSpeed / 3.6f) * dtMax;
        Acceleration = (float)Math.Round(a, 2);
        Throttle = target.GetThrottle();
        float error = SetPoint - currentSpeed;
        float result = kp * error + kd * (error - lastError);
        // logger.LogInfo($"sp={SetPoint} currentSpeed={currentSpeed} throttle={throttle} error={error} result={result} reverser={reverser} accel={accel}");
        // result = Math.Min(result, .2f);
        if (Speed < 5)
        {
            result = (float)Math.Min(.1, result);
        }
        target.SetThrottle(result);
        lastSpeed = currentSpeed;
        lastError = error;
    }
}

class Pid
{

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
        // // obj.Brake?.Set(0f);
        // // obj.IndependentBrake?.Set(1f);
        // // obj.DynamicBrake?.Set(0f);
        // // obj.Handbrake?.Set(0f);
        obj.Throttle?.Set(throttle);
    }

    public float GetReverser()
    {
        TrainCar locoCar = GetLocomotive();
        BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
        // // obj.Brake?.Set(0f);
        // // obj.IndependentBrake?.Set(1f);
        // // obj.DynamicBrake?.Set(0f);
        // // obj.Handbrake?.Set(0f);
        return obj.Reverser.Value;
    }

    public float GetThrottle()
    {
        TrainCar locoCar = GetLocomotive();
        BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
        // // obj.Brake?.Set(0f);
        // // obj.IndependentBrake?.Set(1f);
        // // obj.DynamicBrake?.Set(0f);
        // // obj.Handbrake?.Set(0f);
        // obj.Throttle?.Set(.4f);
        return obj.Throttle.Value;
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