namespace DriverAssist
{
    public class LocoStatsSystem : BaseSystem
    {
        private readonly LocoController loco;
        private readonly RollingSample integrator;
        private readonly float deltaTime;
        private readonly int samples;

        public LocoStatsSystem(LocoController loco, float period, float deltaTime)
        {
            this.loco = loco;
            this.deltaTime = deltaTime;
            samples = (int)(period / deltaTime);
            integrator = new RollingSample(samples);
        }

        public override void OnUpdate()
        {
            integrator.Add(loco.RelativeSpeedMs - loco.Components.LocoStats.SpeedMs);
            float acc = integrator.Sum() / (samples * deltaTime);

            loco.Components.LocoStats = new LocoStats()
            {
                AccelerationMs2 = acc,
                SpeedMs = loco.RelativeSpeedMs
            };
        }

        /// https://stackoverflow.com/questions/4353525/floating-point-linear-interpolation
        // float Lerp(float a, float b, float f)
        // {
        //     return a * (1.0f - f) + (b * f);
        // }
    }
}
