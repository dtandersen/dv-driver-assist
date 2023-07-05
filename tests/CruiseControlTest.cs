using CruiseControlPlugin.Algorithm;
using UnityEngine.SocialPlatforms;
using Xunit;

namespace CruiseControlPlugin
{
    public class CruiseControlTest
    {
        private CruiseControl cruiseControl;
        private FakeLocoController loco;
        private FakeAccelerator accelerator;
        private FakeDecelerator decelerator;
        private FakeCruiseControlConfig config;

        public CruiseControlTest()
        {
            config = new FakeCruiseControlConfig();
            loco = new FakeLocoController();
            loco.Reverser = 1;
            accelerator = new FakeAccelerator();
            decelerator = new FakeDecelerator();
            cruiseControl = new CruiseControl(loco, config);
            cruiseControl.Enabled = true;
            cruiseControl.Accelerator = accelerator;
            cruiseControl.Decelerator = decelerator;
        }

        [Fact]
        public void ShouldAccelerate()
        {
            loco.Throttle = 0;

            cruiseControl.DesiredSpeed = 30;

            loco.Speed = 29.9f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);

            loco.Speed = 25;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);

            loco.Speed = 24.9f;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.Throttle);

            loco.Speed = 27.4f;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.2f, loco.Throttle);

            loco.Speed = 27.5f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0f, loco.Throttle);

            loco.Speed = 27.4f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0f, loco.Throttle);

            loco.Speed = 24;
            WhenCruise();
            Assert.Equal("Accelerating to 27.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.Throttle);
        }

        [Fact]
        public void ShouldDecelerate()
        {
            loco.TrainBrake = 0;
            // loco.Reverser = 1;

            cruiseControl.DesiredSpeed = 20;

            loco.Speed = 20.1f;
            WhenCruise();
            Assert.Equal("Decelerating to 17.5 km/h", cruiseControl.Status);
            Assert.Equal(0.1f, loco.TrainBrake);

            // still going above 17.5, should brake
            loco.Speed = 17.6f;
            WhenCruise();
            Assert.Equal("Decelerating to 17.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal(0.2f, loco.TrainBrake);

            loco.Speed = 17.5f;
            WhenCruise();

            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = 17.6f;
            WhenCruise();

            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = 20.1f;
            WhenCruise();

            Assert.Equal(0.1f, loco.TrainBrake);
        }

        [Fact]
        public void ApplyBrakesWhenZeroSetpoint()
        {
            cruiseControl.DesiredSpeed = 0;
            loco.Speed = 20;
            loco.Throttle = 1;

            WhenCruise();

            Assert.Equal(1, loco.TrainBrake);
            Assert.Equal(0, loco.Throttle);
            Assert.Equal("Stop", cruiseControl.Status);
        }

        [Fact]
        public void DisableWhenTrainBrakeAdjusted()
        {
            cruiseControl.DesiredSpeed = 10;
            loco.Speed = 20;
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
            loco.Speed = 20;
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
            loco.Speed = 20;
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
            Assert.Equal("Direction change", cruiseControl.Status);

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
            Assert.Equal("Direction change", cruiseControl.Status);

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
            Assert.Equal("Direction change", cruiseControl.Status);

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
            Assert.Equal("Direction change", cruiseControl.Status);

            // WhenCruise();
            // Assert.True(cruiseControl.Enabled);
        }

        [Fact]
        public void AccelerateInReverse()
        {
            cruiseControl.DesiredSpeed = -10;
            loco.Speed = -4;
            loco.Reverser = 0;
            WhenCruise();
            Assert.Equal("Accelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0.1f, loco.Throttle);
            Assert.Equal(0, loco.TrainBrake);

            loco.Speed = -7.5f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = -4;
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

            loco.Speed = -10f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = -9f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = -11;
            WhenCruise();
            Assert.Equal("Decelerating to 7.5 km/h", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0.1f, loco.TrainBrake);

            loco.Speed = -7.5f;
            WhenCruise();
            Assert.Equal("Coast", cruiseControl.Status);
            Assert.Equal(0, loco.Reverser);
            Assert.Equal(0f, loco.Throttle);
            Assert.Equal(0f, loco.TrainBrake);

            loco.Speed = -11;
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
            loco.Speed = 0;
            loco.Throttle = 1;
            loco.Reverser = .5f;
            // loco.Throttle = 1;
            // cruiseControl.Enabled = true;
            WhenCruise();
            Assert.Equal(0, loco.Throttle);
            Assert.Equal("Idle: Reverser is in neutral", cruiseControl.Status);
        }

        void WhenCruise()
        {
            cruiseControl.Tick();
        }

        public class FakeLocoController : LocoController
        {
            public float Speed { get; set; }
            public float Throttle { get; set; }
            public float TrainBrake { get; set; }
            public float IndBrake { get; set; }
            public float Temperature { get; set; }
            public float Torque { get; set; }
            public float Reverser { get; set; }
            public float Acceleration { get; set; }
            public float Amps { get; set; }
            public float Rpm { get; set; }

            public float PositiveSpeed
            {
                get
                {
                    if (Reverser > 0)
                        return Speed;
                    else
                        return -Speed;
                }
            }
        }

        internal class FakeAccelerator : CruiseControlAlgorithm
        {
            public float DesiredSpeed { get; set; }

            public void Tick(LocoController loco)
            {
                if (loco.PositiveSpeed < DesiredSpeed)
                {
                    loco.Throttle += .1f;
                }
            }
        }

        internal class FakeDecelerator : CruiseControlAlgorithm
        {
            public float DesiredSpeed { get; set; }

            public void Tick(LocoController loco)
            {
                if (loco.PositiveSpeed > DesiredSpeed)
                {
                    loco.TrainBrake += .1f;
                    loco.IndBrake += .1f;
                }
            }
        }

        internal class FakeCruiseControlConfig : CruiseControlConfig
        {
            public int MaxTorque { get; set; }
        }
    }
}
