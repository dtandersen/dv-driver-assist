using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.ECS
{
    public class ControlsChangedSystemTest
    {
        private readonly LocoEntity loco;
        private readonly EntityManager entityManager;
        private readonly ControlsChangedSystem system;

        public ControlsChangedSystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);

            entityManager = new();
            loco = entityManager.Loco = new LocoEntity(1f / 60f);
            entityManager.Loco.UpdateLocomotive(new FakeTrainCarWrapper());
            system = new ControlsChangedSystem(entityManager);
        }

        /// When the indepdentdant brake is applied
        /// Then the controls have changed
        [Fact]
        public void SavesStateOfBrakes()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.LastControls = new LastControls()
            {
                IndBrake = 0
            };
            loco.IndBrake = 0.5f;

            WhenSystemUpdates();

            Assert.True(loco.Components.ControlsChanged);
            Assert.Null(loco.Components.CruiseControl);
        }

        /// When the train brake is applied
        /// Then the controls have changed
        [Fact]
        public void SavsesStateOfBrakes()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.LastControls = new LastControls()
            {
                TrainBrake = 0
            };
            loco.TrainBrake = 0.5f;

            WhenSystemUpdates();

            Assert.True(loco.Components.ControlsChanged);
            Assert.Null(loco.Components.CruiseControl);
        }

        /// When the controls are the same
        /// Then the controls haven't changed
        [Fact]
        public void SavsesStateOfBrakess()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.ControlsChanged = true;
            loco.Components.LastControls = new LastControls()
            {
                TrainBrake = 0,
                IndBrake = 0
            };
            loco.TrainBrake = 0;
            loco.IndBrake = 0;

            WhenSystemUpdates();

            Assert.Null(loco.Components.ControlsChanged);
            Assert.NotNull(loco.Components.CruiseControl);
        }

        [Fact]
        public void NullLoco()
        {
            entityManager.Loco = null;

            WhenSystemUpdates();
        }

        [Fact]
        public void NullControls()
        {
            entityManager.Loco.Components.LastControls = null;

            WhenSystemUpdates();

            Assert.Null(entityManager.Loco.Components.LastControls);
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
