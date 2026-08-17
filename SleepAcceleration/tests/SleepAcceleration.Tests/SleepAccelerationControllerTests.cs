using System;
using System.Collections.Generic;
using SleepAcceleration.Core;
using Xunit;

namespace SleepAcceleration.Tests
{
    public sealed class SleepAccelerationControllerTests
    {
        [Fact]
        public void DoesNotAccelerateBeforeThreeSeconds()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 2.99f, gateway);

            Assert.Empty(gateway.AppliedSpeeds);
        }

        [Fact]
        public void AcceleratesAtExactlyThreeSeconds()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);

            Assert.Equal(new[] { 5f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void LeavingBeforeThresholdResetsTheCountdown()
        {
            var gateway = new FakeGameSpeedGateway(1f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 2f, gateway);
            controller.Tick(false, false, 1f, gateway);
            controller.Tick(true, false, 1.1f, gateway);

            Assert.Empty(gateway.AppliedSpeeds);
        }

        [Fact]
        public void PausedTimeDoesNotCountTowardTheThreshold()
        {
            var gateway = new FakeGameSpeedGateway(1f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 2f, gateway);
            controller.Tick(true, true, 10f, gateway);
            controller.Tick(true, false, 1f, gateway);

            Assert.Equal(new[] { 5f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void LeavingAfterAccelerationRestoresCapturedUserSpeed()
        {
            var gateway = new FakeGameSpeedGateway(4f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);
            controller.Tick(false, false, 0.1f, gateway);

            Assert.Equal(new[] { 5f, 4f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void UserSpeedChangeCancelsTheActiveAccelerationWithoutRestoringOldSpeed()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);
            gateway.UserSelectedSpeed = 4f;
            controller.NotifyUserSpeedChanged();
            controller.Tick(true, false, 30f, gateway);
            controller.Tick(false, false, 0.1f, gateway);

            Assert.Equal(new[] { 5f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void SuppressionClearsAfterLeavingAndAllowsAnotherAcceleration()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);
            gateway.UserSelectedSpeed = 4f;
            controller.NotifyUserSpeedChanged();
            controller.Tick(false, false, 0.1f, gateway);
            controller.Tick(true, false, 3f, gateway);
            controller.Tick(false, false, 0.1f, gateway);

            Assert.Equal(new[] { 5f, 5f, 4f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void RepeatedSleepingTicksDoNotApplyFiveTimesAgain()
        {
            var gateway = new FakeGameSpeedGateway(1f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);
            controller.Tick(true, false, 20f, gateway);

            Assert.Equal(new[] { 5f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void ResetRestoresActiveAccelerationOnlyOnce()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();

            controller.Tick(true, false, 3f, gateway);
            controller.Reset(gateway);
            controller.Reset(gateway);

            Assert.Equal(new[] { 5f, 2f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void FailedRestoreCanBeRetriedOnTheNextTick()
        {
            var gateway = new FakeGameSpeedGateway(2f);
            var controller = new SleepAccelerationController();
            controller.Tick(true, false, 3f, gateway);
            gateway.ThrowOnceForSpeed = 2f;

            Assert.Throws<InvalidOperationException>(() => controller.Tick(false, false, 0.1f, gateway));
            controller.Tick(false, false, 0.1f, gateway);

            Assert.Equal(new[] { 5f, 2f }, gateway.AppliedSpeeds);
        }

        [Fact]
        public void FailedAccelerationCanBeRetriedWithoutRestartingTheCountdown()
        {
            var gateway = new FakeGameSpeedGateway(1f) { ThrowOnceForSpeed = 5f };
            var controller = new SleepAccelerationController();

            Assert.Throws<InvalidOperationException>(() => controller.Tick(true, false, 3f, gateway));
            controller.Tick(true, false, 0f, gateway);

            Assert.Equal(new[] { 5f }, gateway.AppliedSpeeds);
        }

        private sealed class FakeGameSpeedGateway : IGameSpeedGateway
        {
            public FakeGameSpeedGateway(float userSelectedSpeed)
            {
                UserSelectedSpeed = userSelectedSpeed;
            }

            public float UserSelectedSpeed { get; set; }

            public float? ThrowOnceForSpeed { get; set; }

            public List<float> AppliedSpeeds { get; } = new List<float>();

            public void SetTemporarySpeed(float speed)
            {
                if (ThrowOnceForSpeed.HasValue && ThrowOnceForSpeed.Value == speed)
                {
                    ThrowOnceForSpeed = null;
                    throw new InvalidOperationException("Injected speed failure.");
                }

                AppliedSpeeds.Add(speed);
            }
        }
    }
}
