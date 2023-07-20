using System;

namespace DriverAssist.Cruise
{
    public class PidAcceleration : CruiseControlAlgorithm
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

        // CruiseControlTarget target = null;
        bool Enabled { get; set; }
        float lastThrottle;
        private Pid throttlePid = new Pid(0, 0, 0, 0);
        private Pid torquePid = new Pid(0, 0, 0, 0);
        float currentTime;
        float dt;
        float dtMax = 1f;
        float lastSpeed = 0;
        float lastTorque = 0;

        public PidAcceleration()
        {
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;
            // logger.LogInfo("tick");
            currentTime = 0;
            dt = currentTime - lastThrottle;
            // if (!target.IsLoco())
            // {
            //     Enabled = false;
            //     return;
            // }

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
                loco.Throttle = 0;
                Enabled = false;
                return;
            }
            float reverser = loco.Reverser;
            if (reverser <= 0.5f)
            {
                loco.Throttle = 0;
                Enabled = false;
                return;
            }
            if (dt < dtMax)
            {
                return;
            }
            lastThrottle = 0;
            float currentSpeed = loco.RelativeSpeedKmh;
            double accel = (currentSpeed / 3.6f - lastSpeed / 3.6f) * dtMax;
            Speed = currentSpeed;
            Acceleration = (float)Math.Round(accel, 2);
            Throttle = loco.Throttle;
            Mass = loco.Mass;
            Power = Mass * 9.8f / 2f * Speed / 3.6f;
            Force = Mass * 9.8f / 2f;
            Hoursepower = Power / 745.7f;
            Torque = loco.Torque;

            torquePid.SetPoint = DesiredSpeed;
            float torqueResult = torquePid.evaluate(currentSpeed);
            torqueResult = Math.Min(torqueResult, DesiredTorque);

            throttlePid.SetPoint = torqueResult;
            float throttleResult = throttlePid.evaluate(Torque) / 100f;
            Temperature = loco.Temperature; ;
            // if (throttleResult > 1 || throttleResult < 0)
            // {
            //     throttlePid.Unwind();
            // }
            float step = 0.1f;

            if (Speed > DesiredSpeed)
            {
                throttleResult = 0;
            }
            // else if (target.TooHot())
            // {
            //     throttleResult = Throttle - step;
            // }
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

            loco.Throttle = throttleResult;
            lastSpeed = currentSpeed;
            lastTorque = Torque;

            // logger.LogInfo($"torquePid={torquePid}, throttlePid={throttlePid}");
        }
    }
}