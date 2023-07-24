using System;

namespace DriverAssist
{
    class Pid
    {
        public float SetPoint { get; set; }
        public float Kp { get; }
        public float Kd { get; }
        public float Ki { get; }
        public float Result { get; protected set; }
        public float Error { get; protected set; }
        public float Pv { get; protected set; }
        public float Bias { get; internal set; }
        // public float MinInt { get; set; }
        // public float MaxInt { get; internal set; }
        public float Iterm { get; internal set; }
        public float Pterm { get; internal set; }

        private float lastError;
        private float sum;

        public Pid(float setPoint, float kp, float kd, float ki)
        {
            this.SetPoint = setPoint;
            this.Kp = kp;
            this.Kd = kd;
            this.Ki = ki;
        }

        public float Evaluate(float pv)
        {
            Pv = pv;
            Error = SetPoint - pv;
            Pterm = Kp * Error;
            sum += Error;
            Iterm = Ki * sum;
            if (Math.Abs(Iterm) > Math.Abs(Pterm))
            {
                sum *= 0.95f;
            }
            // if (sum <= 0)
            // {
            //     sum = 0;
            // }
            Result = Pterm + Kd * (Error - lastError) + Iterm + Bias;

            lastError = Error;

            return Result;
        }
        public override string ToString()
        {
            return $"Pid [SetPoint={SetPoint}, Pv={Pv}, Error={Error}, Kp={Pterm}, Kd={Kd}, Ki={Iterm}, sum={sum} Result={Result}]";
        }

        internal void Unwind()
        {
            sum = 0;
        }
    }
}
