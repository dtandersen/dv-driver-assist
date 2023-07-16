using System;
using System.Collections.Generic;
using DriverAssist.Localization;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    public class CruiseControlTest
    {
        private CruiseControl cruiseControl;
        private LocoController loco;
        // private FakeLocoController loco;
        private Translation localization;
        private FakeCruiseControlConfig config;
        private LocoSettings de2settings;
        private LocoSettings dh4settings;
        private FakeTrainCarWrapper train;
        private FakeClock clock;

        public CruiseControlTest(ITestOutputHelper output)
        {
            PluginLoggerSingleton.Instance = new TestLogger(output);
            TranslationManager.Init();
            localization = TranslationManager.Current;
            config = new FakeCruiseControlConfig();
            config.Offset = -2.5f;
            config.Diff = 2.5f;
            config.Acceleration = "DriverAssist.Cruise.FakeAccelerator";
            config.Deceleration = "DriverAssist.Cruise.FakeDecelerator";

            de2settings = new FakeLocoConfig();
            dh4settings = new FakeLocoConfig();
            config.LocoSettings[LocoType.DE2] = de2settings;
            config.LocoSettings[LocoType.DH4] = dh4settings;
            train = new FakeTrainCarWrapper();
            loco = new LocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            // loco = new FakeLocoController();
            train.LocoType = LocoType.DE2;
            loco.Reverser = 1;
            // loco.AccelerationMs = 0;
            // accelerator = new FakeAccelerator();
            // decelerator = new FakeDecelerator();
            clock = new FakeClock();
            cruiseControl = new CruiseControl(loco, config, clock);
            cruiseControl.Enabled = true;
            // cruiseControl.Accelerator = accelerator;
            // cruiseControl.Decelerator = decelerator;
        }

        [Fact]
        public void ShouldAccelerate()
        {
            loco.Throttle = 0;

            cruiseControl.DesiredSpeed = 30;

            train.SpeedKmh = 29.9f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);

            train.SpeedKmh = 25;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);

            train.SpeedKmh = 24.9f;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.Throttle);

            train.SpeedKmh = 27.4f;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.2f, loco.Throttle);

            train.SpeedKmh = 27.5f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0f, loco.Throttle);

            train.SpeedKmh = 27.4f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0f, loco.Throttle);

            train.SpeedKmh = 24;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.Throttle);
        }

        [Fact]
        public void AccelerationNearZero()
        {
            config.Offset = 0;
            config.Diff = 2.5f;
            cruiseControl.DesiredSpeed = 5;

            train.SpeedKmh = 0;
            WhenCruise();
            Assert.Equal("Accelerating to 5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.Throttle);
        }

        [Fact]
        public void ShouldDecelerate()
        {
            loco.TrainBrake = 0;
            cruiseControl.DesiredSpeed = 20;

            train.SpeedKmh = 20.1f;
            WhenCruise();
            Assert.Equal("Decelerating to 17.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.TrainBrake);

            // still going above 17.5, should brake
            train.SpeedKmh = 17.6f;
            WhenCruise();
            Assert.Equal("Decelerating to 17.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(0.2f, loco.TrainBrake);

            train.SpeedKmh = 17.5f;
            WhenCruise();

            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = 17.6f;
            WhenCruise();

            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = 20.1f;
            WhenCruise();

            Assert.Equal(0.1f, loco.TrainBrake);
        }

        [Fact]
        public void ApplyBrakesWhenZeroSetpoint()
        {
            cruiseControl.DesiredSpeed = 0;
            train.SpeedKmh = 20;
            loco.Throttle = 1;

            WhenCruise();

            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(localization.CC_STOPPING, cruiseControl.Status);
        }

        [Fact]
        public void DisableWhenTrainBrakeAdjusted()
        {
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 20;
            WhenCruise();
            Assert.True(cruiseControl.Enabled);
            loco.TrainBrake = 0;
            WhenCruise();
            Assert.False(cruiseControl.Enabled);
        }

        [Fact]
        public void DisableWhenIndBrakeAdjusted()
        {
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 20;
            WhenCruise();
            Assert.True(cruiseControl.Enabled);
            loco.IndBrake = 0;
            WhenCruise();
            Assert.False(cruiseControl.Enabled);
        }

        [Fact]
        public void DontDoAnythingIfDisabled()
        {
            cruiseControl.Enabled = false;
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 20;
            WhenCruise();
            Assert.Equal(0, loco.IndBrake);
            Assert.Equal("Disabled", cruiseControl.Status);
        }

        [Fact]
        public void HaltTrainIfDesiredSpeedIsNegativeAndTrainIsInForwardGear()
        {
            cruiseControl.DesiredSpeed = -10;
            // loco.Reverser=1;
            loco.Throttle = 0.5f;
            // cruiseControl.Enabled = true;
            WhenCruise();
            // Assert.True(cruiseControl.Enabled);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(localization.CC_CHANGING_DIRECTION, cruiseControl.Status);

            WhenCruise();
            Assert.True(cruiseControl.Enabled);
        }

        [Fact]
        public void HaltTrainIfDesiredSpeedIsPositiveAndTrainIsInReverse()
        {
            cruiseControl.DesiredSpeed = 10;
            loco.Reverser = 0;
            loco.Throttle = 0.5f;
            // cruiseControl.Enabled = true;
            WhenCruise();
            // Assert.True(cruiseControl.Enabled);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(localization.CC_CHANGING_DIRECTION, cruiseControl.Status);

            WhenCruise();
            Assert.True(cruiseControl.Enabled);
        }

        [Fact]
        public void ChangeFromReverseToForward()
        {
            cruiseControl.DesiredSpeed = 10;
            loco.Reverser = 0;
            // loco.Throttle = 0.5f;
            // cruiseControl.Enabled = true;
            WhenCruise();
            // Assert.True(cruiseControl.Enabled);
            Assert.Equal(1, loco.Reverser);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(localization.CC_CHANGING_DIRECTION, cruiseControl.Status);

            // WhenCruise();
            // Assert.True(cruiseControl.Enabled);
        }

        [Fact]
        public void ChangeFromForwardToReverse()
        {
            cruiseControl.DesiredSpeed = -10;
            loco.Reverser = 1;
            // loco.Throttle = 0.5f;
            // cruiseControl.Enabled = true;
            WhenCruise();
            // Assert.True(cruiseControl.Enabled);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(localization.CC_CHANGING_DIRECTION, cruiseControl.Status);

            // WhenCruise();
            // Assert.True(cruiseControl.Enabled);
        }

        [Fact]
        public void AccelerateInReverse()
        {
            cruiseControl.DesiredSpeed = -10;
            train.SpeedKmh = -4;
            loco.Reverser = 0;
            WhenCruise();
            Assert.Equal("Accelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0.1f, loco.Throttle);
            Assert.Equal(0, loco.TrainBrake);

            train.SpeedKmh = -7.5f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = -4;
            WhenCruise();
            Assert.Equal("Accelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0.1f, loco.Throttle);
            Assert.Equal(0, loco.TrainBrake);
        }

        [Fact]
        public void DecelerateInReverse()
        {
            cruiseControl.DesiredSpeed = -10;
            loco.Reverser = 0;

            train.SpeedKmh = -10f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = -9f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = -11;
            WhenCruise();
            Assert.Equal("Decelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0.1f, loco.TrainBrake);

            train.SpeedKmh = -7.5f;
            WhenCruise();
            Assert.Equal("Coasting", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            train.SpeedKmh = -11;
            WhenCruise();
            Assert.Equal("Decelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0.1f, loco.TrainBrake);
        }

        [Fact]
        public void ZeroThrottleIfInNeutral()
        {
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 0;
            loco.Throttle = 1;
            loco.Reverser = .5f;
            // loco.Throttle = 1;
            // cruiseControl.Enabled = true;
            WhenCruise();
            Assert.Equal(0, loco.Throttle);
            Assert.Equal("Idle: Engage reverser to start", cruiseControl.Status);
        }

        [Fact]
        public void UseTheSettingsForTheLocoType()
        {
            train.LocoType = LocoType.DH4;
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 0;

            WhenCruise();

            Assert.Equal(dh4settings, ((FakeAccelerator)cruiseControl.Accelerator).Settings);
        }

        [Fact]
        public void ReportIfLocoHasNoSettings()
        {
            train.LocoType = LocoType.DE6;

            WhenCruise();

            Assert.Equal(String.Format(localization.CC_UNSUPPORTED, LocoType.DE6), cruiseControl.Status);
        }

        [Fact]
        public void PassTimeToContext()
        {
            train.LocoType = LocoType.DH4;
            cruiseControl.DesiredSpeed = 10;
            train.SpeedKmh = 0;
            clock.Time2 = 5;

            WhenCruise();

            Assert.Equal(5, ((FakeAccelerator)cruiseControl.Accelerator).Context.Time);
        }

        void WhenCruise()
        {
            cruiseControl.Tick();
        }

        //     public class FakeLocoController : LocoController
        //     {
        //         public string LocoType { get; set; }
        //         public float SpeedKmh { get; set; }

        //         public float RelativeSpeedKmh
        //         {
        //             get
        //             {
        //                 if (IsForward)
        //                     return SpeedKmh;
        //                 else
        //                     return -SpeedKmh;
        //             }
        //         }

        //         public float Throttle { get; set; }
        //         public float TrainBrake { get; set; }
        //         public float IndBrake { get; set; }
        //         public float Temperature { get; set; }
        //         public float Torque { get; set; }
        //         public float RelativeTorque
        //         {
        //             get
        //             {
        //                 if (IsForward) return Torque;
        //                 else return -Torque;
        //             }
        //         }
        //         public float Reverser { get; set; }
        //         public float AmpsRoc { get; set; }
        //         public float AverageAmps { get; set; }
        //         public float Amps { get; set; }
        //         public float Rpm { get; set; }
        //         public float AccelerationMs { get; set; }
        //         public bool IsElectric { get; set; }

        //         public bool IsForward
        //         {
        //             get
        //             {
        //                 return Reverser >= 0.5f;
        //             }
        //         }

        //         public bool IsReversing
        //         {
        //             get
        //             {
        //                 return !IsForward;
        //             }
        //         }

        //         public float RelativeAccelerationMs
        //         {
        //             get
        //             {
        //                 if (IsForward)
        //                     return AccelerationMs;
        //                 else
        //                     return -AccelerationMs;
        //             }
        //         }
        //     }
        // }

    }

    public class FakeCruiseControlConfig : CruiseControlSettings
    {
        public FakeCruiseControlConfig()
        {
            LocoSettings = new Dictionary<string, LocoSettings>();
        }

        public int MinTorque { get; set; }
        public int MinAmps { get; }
        public int MaxAmps { get; }
        public int MaxTemperature { get; }
        public int OverdriveTemperature { get; }
        public bool OverdriveEnabled { get; }
        public float Offset { get; set; }
        public float Diff { get; set; }
        public float UpdateInterval { get; set; }
        public string Acceleration { get; set; }
        public string Deceleration { get; set; }
        public Dictionary<string, LocoSettings> LocoSettings { get; }
    }

    public class FakeLocoConfig : LocoSettings
    {
        public int MinTorque { get; set; }
        public int MinAmps { get; set; }
        public int MaxAmps { get; set; }
        public int MaxTemperature { get; set; }
        public int HillClimbTemp { get; set; }
        public bool OverdriveEnabled { get; set; }
        public int BrakingTime { get; set; }
        public float BrakeReleaseFactor { get; set; }
        public float MinBrake { get; set; }
        public float HillClimbAccel { get; set; }
        public float CruiseAccel { get; set; }
        public float MaxAccel { get; set; }
    }

    public class FakeClock : Clock
    {
        public float Time2 { get; set; }
    }
}