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
            float minTorque;
            float amps = loco.Amps; //.AverageAmps + 0.05f * loco.AmpsRoc;
            bool ampsdecreased = amps <= lastAmps;
            float maxtemp;
            bool overDriveEnabled = context.Config.OverdriveEnabled;
            float minAmps;
            if (overDriveEnabled && loco.Acceleration < 0)
            {
                minTorque = context.Config.MinTorque;
                maxtemp = context.Config.OverdriveTemperature;
                minAmps = context.Config.MinAmps;
                maxamps = context.Config.MaxAmps;
            }
            else
            {
                minTorque = context.Config.MinTorque;
                maxtemp = context.Config.MaxTemperature;
                minAmps = context.Config.MinAmps;
                maxamps = context.Config.MaxAmps;
            }

            if (speed > desiredSpeed)
            {
                throttleResult = 0;
            }
            else if (loco.Temperature > maxtemp)
            {
                throttleResult = throttle - step;
            }
            else if (amps < minAmps)
            {
                throttleResult = throttle + step;
            }
            else if (amps > maxamps)
            {
                throttleResult = throttle - step;
            }
            else if (ampsdecreased && torque < minTorque)
            {
                throttleResult = throttle + step;
            }
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
