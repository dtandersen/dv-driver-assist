using DriverAssist.ECS;
using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    public class DecelTest
    {
        private readonly FakeLocoController loco;
        private readonly FakeLocoConfig de2settings;
        private readonly FakeLocoConfig dm3settings;
        private readonly FakeTrainCarWrapper train;
        private readonly PredictiveDeceleration accelerator;
        private CruiseControlContext context;
        private const float STEP = 1 / 11f;


        public DecelTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);

            de2settings = new FakeLocoConfig
            {
                CruiseAccel = .05f,
                MaxAccel = .25f,
                BrakeReleaseFactor = .5f,
                BrakingTime = 10,
                HillClimbAccel = .025f,
                MaxAmps = 750,
                OperatingTemp = 105,
                HillClimbTemp = 118,
                MinBrake = 0.1f,
                MinTorque = 22000,
                OverdriveEnabled = true
            };

            dm3settings = new FakeLocoConfig
            {
                CruiseAccel = .05f,
                MaxAccel = .25f,
                BrakeReleaseFactor = .5f,
                BrakingTime = 10,
                HillClimbAccel = .025f,
                MaxAmps = 0,
                OperatingTemp = 105,
                HillClimbTemp = 118,
                MinBrake = 0,
                MinTorque = 35000,
                OverdriveEnabled = true
            };
            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2
            };
            loco = new FakeLocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            train.Type = LocoType.DE2;
            train.Length = 2;
            train.IndBrake = 1;
            loco.Reverser = 1;
            accelerator = new PredictiveDeceleration();
            context = new CruiseControlContext(de2settings, loco);
        }

        /// <summary> 
        /// A DE2 isn't decelerating enough to slow down,
        /// The train brake is 50%.
        /// TrainBrake should be 60%.
        /// </summary>
        [Fact]
        public void AccelerateFromStop4()
        {
            context.DesiredSpeed = 5;
            train.SpeedKmh = 6;
            train.TrainBrake = 0.5f;

            WhenDecel();
            Assert.Equal(0.5f + STEP, loco.TrainBrake);
            Assert.Equal(0, loco.IndBrake);
        }

        /// <summary> 
        /// A DE2 is is decelerating enough to slow down,
        /// and MinBrake is .1.
        /// TrainBrake should be 0.
        /// </summary>
        [Fact]
        public void AccelerateFromStop2()
        {
            de2settings.MinBrake = 0.1f;
            context.DesiredSpeed = 5;
            loco.AccelerationMs = -1;
            train.TrainBrake = 0;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0.1f, loco.TrainBrake);
            Assert.Equal(0, loco.IndBrake);
        }


        /// <summary> 
        /// A DE2 is is decelerating enough to slow down,
        /// and TrainBrake 1
        /// and BrakeReleaseFactor is .5
        /// TrainBrake should be .5.
        /// </summary>
        [Fact]
        public void ShouldReduceBrakesByBrakeFactor()
        {
            de2settings.BrakeReleaseFactor = 0.5f;
            context.DesiredSpeed = 5;
            loco.AccelerationMs = -1;
            train.TrainBrake = 1;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0.5f, loco.TrainBrake);
            Assert.Equal(0, loco.IndBrake);
        }

        /// <summary> 
        /// A DE2 is is decelerating enough to slow down,
        /// and TrainBrake .2
        /// and BrakeReleaseFactor is .9
        /// TrainBrake should be .1.
        /// </summary>
        [Fact]
        public void AlwaysAppliesMinBrake()
        {
            de2settings.BrakeReleaseFactor = 0.9f;
            context.DesiredSpeed = 5;
            loco.AccelerationMs = -1;
            train.TrainBrake = .2f;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0.1f, loco.TrainBrake);
            Assert.Equal(0, loco.IndBrake);
        }

        /// <summary> 
        /// A DM3 is is decelerating enough to slow down,
        /// and MinBrake is .1.
        /// TrainBrake should be 0.
        /// </summary>
        [Fact]
        public void Dm3ReleasesBrake()
        {
            dm3settings.MinBrake = 0.1f;
            context = new CruiseControlContext(dm3settings, loco);
            train.Type = LocoType.DM3;
            context.DesiredSpeed = 5;
            loco.AccelerationMs = -1;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0, loco.TrainBrake);
            Assert.Equal(0, loco.IndBrake);
        }

        /// <summary> 
        /// A DM3 isn't decelerating enough to slow down,
        /// TrainBrake should be .666.
        /// </summary>
        [Fact]
        public void Dm3AppliesLappingBrake()
        {
            dm3settings.MinBrake = 0.1f;
            context = new CruiseControlContext(dm3settings, loco);
            train.Type = LocoType.DM3;
            context.DesiredSpeed = 5;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0.666f, loco.TrainBrake, 2);
            Assert.Equal(0, loco.IndBrake);
        }

        /// <summary> 
        /// A train of Length 1 isn't decelerating enough to slow down,
        /// and IndBrake is 0.
        /// IndBrake should be .1.
        /// </summary>
        [Fact]
        public void SingleCarTrainAppliesIndependantBrake()
        {
            context.DesiredSpeed = 5;
            train.IndBrake = STEP;
            train.TrainBrake = 1;
            train.SpeedKmh = 6;
            train.Length = 1;

            WhenDecel();
            Assert.Equal(2 * STEP, loco.IndBrake);
            Assert.Equal(0, loco.TrainBrake);
        }

        /// <summary> 
        /// A train of Length 1 isn't decelerating enough to slow down,
        /// and IndBrake is 0.
        /// IndBrake should be .1.
        /// </summary>
        [Fact]
        public void SingleCarTrainReleasesIndependantBrake()
        {
            context.DesiredSpeed = 5;
            de2settings.BrakeReleaseFactor = .6f;
            de2settings.MinBrake = .1f;
            train.IndBrake = 2 * STEP;
            train.TrainBrake = 1;
            train.SpeedKmh = 6;
            loco.AccelerationMs = -1;
            train.Length = 1;

            WhenDecel();
            Assert.Equal(de2settings.MinBrake, loco.IndBrake, 3);
            Assert.Equal(0, loco.TrainBrake);
        }

        void WhenDecel()
        {
            accelerator.Tick(context);
            context.Time += 1;
        }
    }
}
