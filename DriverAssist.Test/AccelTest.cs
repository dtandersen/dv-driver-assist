using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DriverAssist.Localization;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    public class AccelTest
    {
        // private CruiseControl cruiseControl;
        private FakeLocoController loco;
        // private FakeLocoController loco;
        private Translation localization;
        private FakeCruiseControlConfig config;
        private FakeLocoConfig de2settings;
        private FakeLocoConfig dh4settings;
        private FakeTrainCarWrapper train;
        private PredictiveAcceleration accelerator;
        private CruiseControlContext context;
        float step = 1 / 11f;

        private readonly ITestOutputHelper output;

        public AccelTest(ITestOutputHelper output)
        {
            this.output = output;
            PluginLoggerSingleton.Instance = new TestLogger(output);
            // TranslationManager.Init();
            // localization = TranslationManager.Current;
            // config = new FakeCruiseControlConfig();
            // config.Offset = -2.5f;
            // config.Diff = 2.5f;
            // config.Acceleration = "DriverAssist.Cruise.FakeAccelerator";
            // config.Deceleration = "DriverAssist.Cruise.FakeDecelerator";

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
            // config.LocoSettings[LocoType.DE2] = de2settings;
            // config.LocoSettings[LocoType.DH4] = dh4settings;
            train = new FakeTrainCarWrapper();
            train.LocoType = LocoType.DE2;
            loco = new FakeLocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            // loco = new FakeLocoController();
            train.LocoType = LocoType.DE2;
            loco.Reverser = 1;
            // loco.AccelerationMs = 0;
            // accelerator = new FakeAccelerator();
            // decelerator = new FakeDecelerator();
            // cruiseControl = new CruiseControl(loco, config);
            // cruiseControl.Enabled = true;
            accelerator = new PredictiveAcceleration();
            accelerator.lastShift = -3;
            // cruiseControl.Decelerator = decelerator;
            context = new CruiseControlContext(de2settings, loco);
        }

        ~AccelTest()
        {
            PluginLoggerSingleton.Instance = new NullLogger();
        }

        /// <summary> 
        /// The train is stopped.
        /// It should speed up.
        /// </summary>
        [Fact]
        public void AccelerateFromStop()
        {
            // loco.Throttle = 1;
            context.Time = 5;

            // cruiseControl.DesiredSpeed = 30;
            context.DesiredSpeed = 5;

            train.SpeedKmh = 0;
            // train.acc
            WhenAccel();
            // Assert.Equal("Coasting", cruiseControl.Status);
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
            // context.Time = 5;
            loco.AccelerationMs = 0.25f;
            train.Throttle = step;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            // loco.AccelerationMs = 0.25f;
            train.Throttle = step;
            train.Torque = 10000;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Temperature = 105;
            loco.TemperatureChange = 0;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            loco.AccelerationMs = de2settings.HillClimbAccel;
            // loco.TemperatureChange = 0.5f;
            train.Throttle = 3 * step;
            // train.Torque = 10000;
            train.Temperature = de2settings.MaxTemperature;
            // train.SpeedKmh = 0;

            WhenAccel();

            Assert.Equal(4 * step, loco.Throttle);
        }

        // /// <summary>
        // /// The train is accelerating slowly,
        // /// the temperature is dangerously high,
        // /// It should slow down.
        // /// </summary>
        // [Fact]
        // public void SpeedUpWhenAccelerationIsLow2()
        // {
        //     context.DesiredSpeed = 5;
        //     // context.Time = 5;
        //     loco.AccelerationMs = de2settings.HillClimbAccel;
        //     loco.TemperatureChange = 0.5f;
        //     train.Throttle = 3 * step;
        //     // train.Torque = 10000;
        //     train.Temperature = de2settings.MaxTemperature - 0.5f;
        //     // train.SpeedKmh = 0;

        //     WhenAccel();

        //     Assert.Equal(4 * step, loco.Throttle);
        // }

        /// <summary> 
        /// The train is above operating temperature it should maintain the current throttle position if the temperature is decreasing.
        /// </summary>
        [Fact(Skip = "Duplicates MaintainThrottleIfTemperatureIsFalling")]
        public void DontThrottleUpIfCruising()
        {
            context.Time = 5;
            accelerator.lastShift = 0;
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = 0.05f;
            loco.TemperatureChange = -.1f;
            // train.I
            train.Throttle = 2 * step;
            train.Torque = 15000;
            train.Temperature = 106;
            // train.Amps = 750;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            //  loco.AccelerationMs = 0.25f;
            loco.TemperatureChange = 0;
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.Temperature = 118;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            //  loco.AccelerationMs = 0.25f;
            loco.TemperatureChange = -.1f;
            // train.I
            train.Throttle = 3 * step;
            train.Torque = 10000;
            // train.Temperature = 118;
            train.Amps = 750;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            //  loco.AccelerationMs = 0.25f;

            loco.TemperatureChange = -.1f;
            // train.I
            train.Throttle = 3 * step;
            train.Torque = 10000;
            train.LocoType = LocoType.DH4;
            // train.Temperature = 118;
            train.Amps = 0;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            //  loco.AccelerationMs = 0.25f;
            // loco.TemperatureChange = -.1f;
            // train.I
            train.Throttle = 2 * step;
            train.Torque = 25000;
            // train.Temperature = 118;
            // train.Amps = 750;
            // train.SpeedKmh = 0;

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
            // context.Time = 5;
            //  loco.AccelerationMs = 0.25f;
            // loco.TemperatureChange = -.1f;
            // train.I
            train.Throttle = 2 * step;
            train.Torque = 25000;
            // train.Temperature = 118;
            // train.Amps = 750;
            // train.SpeedKmh = 0;

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
        // override public float RelativeAccelerationMs { get; set; }

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
