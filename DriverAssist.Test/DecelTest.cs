using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DriverAssist.Localization;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    [Collection("Sequential")]
    public class DecelTest
    {
        private FakeLocoController loco;
        private Translation localization;
        private FakeCruiseControlConfig config;
        private FakeLocoConfig de2settings;
        private FakeLocoConfig dh4settings;
        private FakeLocoConfig dm3settings;
        private FakeTrainCarWrapper train;
        private PredictiveDeceleration accelerator;
        private CruiseControlContext context;
        float step = 1 / 11f;


        public DecelTest(ITestOutputHelper output)
        {
            PluginLoggerSingleton.Instance = new TestLogger(output);

            de2settings = new FakeLocoConfig();
            de2settings.CruiseAccel = .05f;
            de2settings.MaxAccel = .25f;
            de2settings.BrakeReleaseFactor = .5f;
            de2settings.BrakingTime = 10;
            de2settings.CruiseAccel = .05f;
            de2settings.HillClimbAccel = .025f;
            de2settings.MaxAmps = 750;
            de2settings.MaxTemperature = 105;
            de2settings.HillClimbTemp = 118;
            de2settings.MinAmps = 400;
            de2settings.MinBrake = 0.1f;
            de2settings.MinTorque = 22000;
            de2settings.OverdriveEnabled = true;

            dh4settings = new FakeLocoConfig();
            dh4settings.CruiseAccel = .05f;
            dh4settings.MaxAccel = .25f;
            dh4settings.BrakeReleaseFactor = .5f;
            dh4settings.BrakingTime = 10;
            dh4settings.CruiseAccel = .05f;
            dh4settings.HillClimbAccel = .025f;
            dh4settings.MaxAmps = 0;
            dh4settings.MaxTemperature = 105;
            dh4settings.HillClimbTemp = 118;
            dh4settings.MinAmps = 400;
            dh4settings.MinBrake = 0;
            dh4settings.MinTorque = 35000;
            dh4settings.OverdriveEnabled = true;

            dm3settings = new FakeLocoConfig();
            dm3settings.CruiseAccel = .05f;
            dm3settings.MaxAccel = .25f;
            dm3settings.BrakeReleaseFactor = .5f;
            dm3settings.BrakingTime = 10;
            dm3settings.CruiseAccel = .05f;
            dm3settings.HillClimbAccel = .025f;
            dm3settings.MaxAmps = 0;
            dm3settings.MaxTemperature = 105;
            dm3settings.HillClimbTemp = 118;
            dm3settings.MinAmps = 400;
            dm3settings.MinBrake = 0;
            dm3settings.MinTorque = 35000;
            dm3settings.OverdriveEnabled = true;
            train = new FakeTrainCarWrapper();
            train.LocoType = LocoType.DE2;
            loco = new FakeLocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            train.LocoType = LocoType.DE2;
            train.Length = 2;
            train.IndBrake = 1;
            loco.Reverser = 1;
            accelerator = new PredictiveDeceleration();
            // accelerator.lastShift = -3;
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
            // dm3settings.MinBrake = 0.1f;
            // context = new CruiseControlContext(dm3settings, loco);
            // train.LocoType = DriverAssist.LocoType.DM3;
            context.DesiredSpeed = 5;
            // loco.AccelerationMs = -1;
            train.SpeedKmh = 6;
            train.TrainBrake = 0.5f;

            WhenDecel();
            Assert.Equal(0.5f + step, loco.TrainBrake);
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
            // dm3settings.MinBrake = 0.1f;
            // context = new CruiseControlContext(dm3settings, loco);
            // train.LocoType = DriverAssist.LocoType.DM3;
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
            train.LocoType = DriverAssist.LocoType.DM3;
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
            train.LocoType = DriverAssist.LocoType.DM3;
            context.DesiredSpeed = 5;
            train.SpeedKmh = 6;

            WhenDecel();
            Assert.Equal(0.666f, loco.TrainBrake);
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
            train.IndBrake = step;
            train.TrainBrake = 1;
            train.SpeedKmh = 6;
            train.Length = 1;

            WhenDecel();
            Assert.Equal(2 * step, loco.IndBrake);
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
            train.IndBrake = 2 * step;
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
