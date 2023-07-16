using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DriverAssist.Localization;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    [Collection("Sequential")]
    public class AccelTest
    {
        private FakeLocoController loco;
        private Translation localization;
        private FakeCruiseControlConfig config;
        private FakeLocoConfig de2settings;
        private FakeLocoConfig dh4settings;
        private FakeTrainCarWrapper train;
        private PredictiveAcceleration accelerator;
        private CruiseControlContext context;
        float step = 1 / 11f;


        public AccelTest(ITestOutputHelper output)
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
            de2settings.MinBrake = 0;
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
            train = new FakeTrainCarWrapper();
            train.LocoType = LocoType.DE2;
            loco = new FakeLocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            train.LocoType = LocoType.DE2;
            loco.Reverser = 1;
            accelerator = new PredictiveAcceleration();
            accelerator.lastShift = -3;
            context = new CruiseControlContext(de2settings, loco);
        }

        /// <summary> 
        /// The train is stopped.
        /// It should speed up.
        /// </summary>
        [Fact]
        public void AccelerateFromStop()
        {
            context.Time = 5;

            context.DesiredSpeed = 5;

            train.SpeedKmh = 0;
            WhenAccel();
            Assert.Equal(step, loco.Throttle);
        }

        /// <summary> 
        /// The train is accelerating rapidly,
        /// but the throttle is only in the first notch.
        /// It should maintain speed.
        /// </summary>
        [Fact]
        public void DontExceedMaxAccel()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = 0.25f;
            train.Throttle = step;

            WhenAccel();

            Assert.Equal(step, loco.Throttle);
        }

        /// <summary> 
        /// The train is accelerating rapidly,
        /// and the throttle is past the first notch.
        /// It should slow down.
        /// </summary>
        [Fact]
        public void DontExceedMaxAccel2()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.MaxAccel;
            train.Throttle = 1.1f * step;
            // train.SpeedKmh = 0;

            WhenAccel();

            Assert.Equal(step, loco.Throttle);
        }

        /// <summary> 
        /// Torque is below MinTorque.
        /// The locomotive should speed up.
        /// </summary>
        [Fact]
        public void SpeedUpIfLowTorque()
        {
            context.DesiredSpeed = 5;
            train.Throttle = step;
            train.Torque = 10000;

            WhenAccel();

            Assert.Equal(2 * step, loco.Throttle);

            WhenAccel();
            WhenAccel();
            WhenAccel();

            Assert.Equal(3 * step, loco.Throttle);
        }

        /// <summary> 
        /// The train is above operating temperature,
        /// it is cruising,
        /// and the temperature is not decreasing.
        /// It should slow down.
        /// </summary>
        [Fact]
        public void SlowDownIfHeatIsOverOperatingTemp()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Temperature = 105;
            loco.TemperatureChange = 0;

            WhenAccel();
            Assert.Equal(2 * step, loco.Throttle);

            WhenAccel();
            Assert.Equal(2 * step, loco.Throttle);

            WhenAccel();
            Assert.Equal(2 * step, loco.Throttle);

            WhenAccel();
            Assert.Equal(1 * step, loco.Throttle);
        }

        /// <summary> 
        /// The train is above operating temperature,
        /// it is cruising,
        /// and the temperature is decreasing.
        /// It should maintain the current throttle position.
        /// </summary>
        [Fact]
        public void MaintainThrottleIfTemperatureIsFalling()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Temperature = 106;
            // train.SpeedKmh = 0;

            WhenAccel();

            Assert.Equal(3 * step, loco.Throttle);
        }

        /// <summary>
        /// The train is accelerating slowly
        /// and it has exceeded operating temperature.
        /// It should speed up.
        /// </summary>
        [Fact]
        public void SpeedUpWhenAccelerationIsLow()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = de2settings.HillClimbAccel;
            train.Throttle = 3 * step;
            train.Temperature = de2settings.MaxTemperature;

            WhenAccel();

            Assert.Equal(4 * step, loco.Throttle);
        }

        /// <summary> 
        /// The train is above operating temperature it should maintain the current throttle position if the temperature is decreasing.
        /// </summary>
        [Fact(Skip = "Duplicates MaintainThrottleIfTemperatureIsFalling")]
        public void DontThrottleUpIfCruising()
        {
            context.Time = 5;
            accelerator.lastShift = 0;
            context.DesiredSpeed = 5;
            loco.AccelerationMs = 0.05f;
            loco.TemperatureChange = -.1f;
            train.Throttle = 2 * step;
            train.Torque = 15000;
            train.Temperature = 106;

            WhenAccel();

            Assert.Equal(2 * step, train.Throttle);
        }

        /// <summary> 
        /// The traction motor is about to explode.
        /// The train should slow down.
        /// </summary>
        [Fact]
        public void SlowDownIfTemperatureIsCritical()
        {
            context.DesiredSpeed = 5;
            loco.TemperatureChange = 0;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Temperature = 118;

            WhenAccel();

            Assert.Equal(2 * step, loco.Throttle);
        }

        /// <summary> 
        /// The traction motor is about to explode.
        /// The train should slow down.
        /// </summary>
        [Fact]
        public void SlowDownIfAmpsIsCritical()
        {
            context.DesiredSpeed = 5;
            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Amps = 750;

            WhenAccel();

            Assert.Equal(2 * step, loco.Throttle);
        }

        /// <summary> 
        /// The DE4 has no traction motor,
        /// It should speed up.
        /// </summary>
        [Fact]
        public void DE4HasNoAmps()
        {
            context = new CruiseControlContext(dh4settings, loco);
            context.DesiredSpeed = 5;

            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.LocoType = LocoType.DH4;
            train.Amps = 0;

            WhenAccel();

            Assert.Equal(4 * step, loco.Throttle);
        }

        /// <summary> 
        /// Torque is above MinTorque.
        /// The train should maintain speed.
        /// </summary>
        [Fact]
        public void MaintainThrottleIfTorqueHigh()
        {
            context.DesiredSpeed = 5;
            train.Throttle = 2 * step;
            train.Torque = 25000;

            WhenAccel();

            Assert.Equal(2 * step, loco.Throttle);
        }

        /// <summary> 
        /// The train did not shift.
        /// LastShift should not change.
        /// </summary>
        [Fact]
        public void DontUpdateShiftTimeIfWeDontShift()
        {
            context.Time = 5;
            accelerator.lastShift = 0;
            context.DesiredSpeed = 5;
            train.Throttle = 2 * step;
            train.Torque = 25000;

            WhenAccel();

            Assert.Equal(0, accelerator.lastShift);
        }


        void WhenAccel()
        {
            accelerator.Tick(context);
            context.Time += 1;
        }
    }

    class FakeLocoController : LocoController
    {
        override public float AccelerationMs { get; set; }
        override public float TemperatureChange { get; set; }

        public FakeLocoController(float fixedDeltaTime) : base(fixedDeltaTime)
        {
        }
    }

    public class TestLogger : PluginLogger
    {
        public string Prefix { get; set; }

        public void Info(string message) { output.WriteLine(message); }

        private readonly ITestOutputHelper output;

        public TestLogger(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}
