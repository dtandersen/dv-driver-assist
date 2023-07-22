using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    [Collection("Sequential")]
    public class LocoControllerTest
    {
        private LocoController loco;
        private FakeTrainCarWrapper train;

        public LocoControllerTest(ITestOutputHelper output)
        {
            PluginLoggerSingleton.Instance = new TestLogger(output);

            train = new FakeTrainCarWrapper
            {
                LocoType = DriverAssist.LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoController(1f / 60f);
            loco.UpdateLocomotive(train);
        }

        public void Dispose()
        {
            PluginLoggerSingleton.Instance = new NullLogger();
        }

        /// The train is a DM3
        /// and a gear change has been requested.
        /// Proceed.
        [Fact]
        public void ShiftGearsInDm3()
        {
            train.LocoType = LocoType.DM3;
            train.Throttle = 1;
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.RequestedGear = 3;
            loco.UpdateStats(1 / 60);
            Assert.Equal(0, train.GearboxA);
            Assert.Equal(0, train.GearboxB);
            Assert.Equal(0, train.Throttle);

            loco.UpdateStats(1 / 60);
            Assert.Equal(0.5f, train.GearboxA);
            Assert.Equal(0.5f, train.GearboxB);
            Assert.Equal(0, train.Throttle);

            loco.UpdateStats(1 / 60);
            Assert.Equal(1, train.Throttle);
        }

        /// The train is a DE2
        /// and a gear change has been requested.
        /// Ignore it.
        [Fact]
        public void DoesntAutoShiftIfTrainNotMechanical()
        {
            train.Throttle = 1;
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.RequestedGear = 2;
            loco.UpdateStats(1 / 60);

            Assert.Equal(0, train.GearboxA);
            Assert.Equal(0, train.GearboxB);
            Assert.Equal(1, train.Throttle);
        }

        /// The train is a DE2
        /// and Gear is set.
        /// Ignore it.
        [Fact]
        public void DoesntWriteToGearboxes()
        {
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.Gear = 2;

            Assert.Equal(0, train.GearboxA);
            Assert.Equal(0, train.GearboxB);
        }

        /// The train is a DE2
        /// and Gear is read.
        /// Return -1.
        [Fact]
        public void DoesntReadFromGearboxes()
        {
            train.GearboxA = 1;
            train.GearboxB = 1;

            loco.Gear = 0;

            Assert.Equal(-1, loco.Gear);
        }
    }
}
