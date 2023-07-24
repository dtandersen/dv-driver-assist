#pragma warning disable CS8629

using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    // [Collection("Sequential")]
    public class LocoControllerTest
    {
        private readonly LocoController loco;
        private readonly FakeTrainCarWrapper train;

        public LocoControllerTest(ITestOutputHelper output)
        {
            PluginLoggerSingleton.Instance = new TestLogger(output);

            train = new FakeTrainCarWrapper
            {
                LocoType = LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoController(1f / 60f);
            loco.UpdateLocomotive(train);
        }

        // public void Dispose()
        // {
        //     PluginLoggerSingleton.Instance = new NullLogger();
        // }

        /// The train is a DM3
        /// and a throttle change has been requested,
        /// but a gear change is in progress.
        /// Ignore it.
        [Fact]
        public void PreventsThrottleChangeDuringShift()
        {
            train.LocoType = LocoType.DM3;
            train.Throttle = 0;
            train.GearChangeInProgress = true;
            loco.Throttle = 1;

            Assert.Equal(0, train.Throttle);
        }

        // /// The train is a DM3
        // /// and a gear change has been requested.
        // /// Proceed.
        // [Fact]
        // public void ShiftGearsInDm3()
        // {
        //     train.LocoType = LocoType.DM3;
        //     train.Throttle = 1;
        //     train.GearboxA = 0;
        //     train.GearboxB = 0;

        //     loco.RequestedGear = 3;
        //     loco.UpdateStats(1 / 60);
        //     Assert.Equal(0, train.GearboxA);
        //     Assert.Equal(0, train.GearboxB);
        //     Assert.Equal(0, train.Throttle);

        //     loco.UpdateStats(1 / 60);
        //     Assert.Equal(0.5f, train.GearboxA);
        //     Assert.Equal(0.5f, train.GearboxB);
        //     Assert.Equal(0, train.Throttle);

        //     train.GearChangeInProgress = true;
        //     loco.UpdateStats(1 / 60);
        //     Assert.Equal(0, train.Throttle);

        //     train.GearChangeInProgress = false;
        //     loco.UpdateStats(1 / 60);
        //     Assert.Equal(1, train.Throttle);
        // }

        // /// The train is a DE2
        // /// and a gear change has been requested.
        // /// Ignore it.
        // [Fact]
        // public void DoesntAutoShiftIfTrainNotMechanical()
        // {
        //     train.Throttle = 1;
        //     train.GearboxA = 0;
        //     train.GearboxB = 0;

        //     loco.RequestedGear = 2;
        //     loco.UpdateStats(1 / 60);

        //     Assert.Equal(0, train.GearboxA);
        //     Assert.Equal(0, train.GearboxB);
        //     Assert.Equal(1, train.Throttle);
        // }

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

        /// The train is a DE2
        /// and a gear change is requested.
        /// Deny it.
        [Fact]
        public void DenyDe2Shift()
        {
            // train.GearboxA = 1;
            // train.GearboxB = 1;

            loco.Gear = 0;
            loco.ChangeGear(1);

            // Assert.Null(loco.Components.RequestedGear);
            Assert.Null(loco.Components.GearChangeRequest);
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Fact]
        public void AllowDm3GearShift()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            // loco.Gear = 0;
            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 1;
            loco.ChangeGear(1);

            // Assert.Equal(1, loco.Components.RequestedGear);
            Assert.Equal(1, loco.Components.GearChangeRequest.Value.RequestedGear);
            Assert.Equal(1, loco.Components.GearChangeRequest.Value.RestoreThrottle);
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Fact]
        public void DontRestoreThrottleIfThrottleIsZero()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            // loco.Gear = 0;
            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 0;
            loco.ChangeGear(1);

            // Assert.Equal(1, loco.Components.RequestedGear);
            Assert.Equal(1, loco.Components.GearChangeRequest.Value.RequestedGear);
            Assert.Null(loco.Components.GearChangeRequest.Value.RestoreThrottle);
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Fact]
        public void DoesntShiftIfShifting()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            // loco.Gear = 0;
            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 0;
            var request = new GearChangeRequest
            {
                RequestedGear = 8,
                RestoreThrottle = 1
            };
            loco.Components.GearChangeRequest = request;
            loco.ChangeGear(1);

            // Assert.Equal(1, loco.Components.RequestedGear);
            Assert.Equal(8, loco.Components.GearChangeRequest.Value.RequestedGear);
            Assert.Equal(1, loco.Components.GearChangeRequest.Value.RestoreThrottle);
            // Assert.Null(loco.Components.GearChangeRequest.Value.RestoreThrottle);
        }

        /// The train is a DM3
        /// and Gear is 0
        /// and the gear -1 is requested.
        /// Ignore it.
        [Fact]
        public void CantShiftBelowMinGear()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            loco.Gear = 0;
            loco.ChangeGear(-1);

            // Assert.Null(loco.Components.RequestedGear);
            Assert.Null(loco.Components.GearChangeRequest);
        }

        /// The train is a DM3
        /// and Gear is 8
        /// and the gear 9 is requested.
        /// Ignore it.
        [Fact]
        public void CantShiftAboveMaxGear()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            loco.Gear = 8;
            loco.ChangeGear(9);

            // Assert.Null(loco.Components.RequestedGear);
            Assert.Null(loco.Components.GearChangeRequest);
        }

        /// The train is a DM3
        /// and Gear is 1
        /// and the gear 1 is requested.
        /// Ignore it.
        [Fact]
        public void DoesntShiftToSameGear()
        {
            train.LocoType = DriverAssist.LocoType.DM3;

            loco.Gear = 1;
            loco.ChangeGear(1);

            // Assert.Null(loco.Components.RequestedGear);
            Assert.Null(loco.Components.GearChangeRequest);
        }


    }
}
