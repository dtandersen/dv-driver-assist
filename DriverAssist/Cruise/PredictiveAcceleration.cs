using DV.HUD;

namespace DriverAssist.Cruise
{
    public class PredictiveAcceleration : CruiseControlAlgorithm
    {
        float lastAmps = 0;
        float lastTorque = 0;
        float step = 1f / 11f;
        bool cooling = false;

        public PredictiveAcceleration()
        {
        }

        public void Tick(CruiseControlContext context)
        {
            LocoController loco = context.LocoController;

            float reverser = loco.Reverser;
            float speed = loco.RelativeSpeed;
            float desiredSpeed = context.DesiredSpeed;
            float throttle = loco.Throttle;
            float torque = loco.Torque;
            float temperature = loco.Temperature;
            float ampDelta = loco.Amps - lastAmps;
            float throttleResult;
            float predictedAmps = loco.Amps + ampDelta * 2f;
            float acceleration = loco.RelativeAcceleration;
            float maxamps;
            float maxtorque;
            if (acceleration < 0)
                maxamps = 750;
            else
                maxamps = 600;

            if (acceleration < 0)
                maxtorque = 25000;
            else
                maxtorque = 20000;

            float amps = loco.Amps; //.AverageAmps + 0.05f * loco.AmpsRoc;
            bool ampsdecreased = amps <= lastAmps;
            float maxtemp;
            bool overdrive = true;
            if (!overdrive || loco.Acceleration >= 0)
            {
                maxtemp = 104;
                maxamps = 750;
            }
            else
            {
                maxtemp = 118;
                maxamps = 750;
                // maxamps = 725;
            }
            // if (speed < 5)
            // {
            //     throttleResult = step;
            // }
            if (speed > desiredSpeed)
            {
                throttleResult = 0;
            }
            // else if (cooling && acceleration < 0.5f)
            // {
            //     cooling = false;
            //     throttleResult = throttle + step;
            // }
            else if (amps < 400)
            {
                throttleResult = throttle + step;
            }
            // else if (loco.Acceleration >= 0.1f)
            // {
            //     // if (amps > 400)
            //     // {
            //     throttleResult = throttle - step;
            //     cooling = true;
            //     // }
            //     // else throttleResult = 0;
            // }
            else if (loco.Temperature >= maxtemp)
            {
                throttleResult = throttle - step;
            }
            else if (ampsdecreased && torque < 20000)
            {
                throttleResult = throttle + step;
            }
            else if (amps > maxamps)
            {
                throttleResult = throttle - step;
            }
            // else if (loco.Torque > maxtorque && !(loco.Torque < lastTorque))
            // {
            //     throttleResult = throttle - step;
            // }
            // else if (loco.Amps > maxamps && !(loco.Amps < lastAmps))
            // {
            //     throttleResult = throttle - step;
            // }
            // else if (predictedAmps > maxamps)
            // {
            //     throttleResult = throttle - step;
            // }
            // else if (predictedAmps < maxamps)
            // {
            //     throttleResult = throttle + step;
            // }
            // else if (loco.Torque < maxtorque)
            // {
            //     throttleResult = throttle + step;
            // }
            else
            {
                throttleResult = throttle;
            }

            loco.Throttle = throttleResult;
            loco.IndBrake = 0;
            loco.TrainBrake = 0;

            lastAmps = loco.Amps;
            lastTorque = loco.Torque;
        }
    }
}
