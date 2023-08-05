using System;
using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.ECS
{
    public class LastControlsSystemTest
    {
        private readonly LocoEntity loco;
        private readonly EntityManager entityManager;
        private readonly LastControlsSystem system;

        public LastControlsSystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);

            entityManager = new();
            loco = entityManager.Loco = new LocoEntity(1f / 60f);
            entityManager.Loco.UpdateLocomotive(new FakeTrainCarWrapper());
            system = new LastControlsSystem(entityManager);
        }

        [Fact]
        public void SavesStateOfBrakes()
        {
            loco.IndBrake = 0.5f;
            loco.TrainBrake = 0.6f;

            WhenSystemUpdates();

            Assert.Equal(loco.IndBrake, loco.Components.LastControls.Value.IndBrake);
            Assert.Equal(loco.TrainBrake, loco.Components.LastControls.Value.TrainBrake);
        }

        [Fact]
        public void NullLoco()
        {
            entityManager.Loco = null;

            WhenSystemUpdates();
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
