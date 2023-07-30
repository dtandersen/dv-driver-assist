using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.ECS
{
    public class LocoStatsSystemTest
    {
        private readonly LocoEntity loco;
        private readonly FakeTrainCarWrapper train;
        private readonly LocoStatsSystem system;

        public LocoStatsSystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);

            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoEntity(1f / 60f);
            loco.UpdateLocomotive(train);
            system = new LocoStatsSystem(loco, 1, .5f);
        }

        /// <summary>
        /// The train is a DM3 
        /// and a gear change has been requested.
        /// Proceed.
        /// </summary>
        [Fact]
        public void UpdatesAcceleration()
        {
            loco.Components.LocoStats = new LocoStats()
            {
                SpeedMs = 1
            };
            train.SpeedMs = 2;
            WhenSystemUpdates();

            Assert.Equal(1, loco.Components.LocoStats.AccelerationMs2);
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
