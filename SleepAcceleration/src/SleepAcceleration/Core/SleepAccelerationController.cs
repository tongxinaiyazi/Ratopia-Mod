namespace SleepAcceleration.Core
{
    internal sealed class SleepAccelerationController
    {
        private const float ActivationDelaySeconds = 3f;
        private const float AcceleratedSpeed = 5f;

        private SleepState _state;
        private float _elapsedSeconds;
        private float _restoreSpeed;

        public void Tick(
            bool isSleepingInQueenBed,
            bool isPaused,
            float unscaledDeltaTime,
            IGameSpeedGateway gateway)
        {
            if (!isSleepingInQueenBed)
            {
                LeaveBed(gateway);
                return;
            }

            if (_state == SleepState.Suppressed || _state == SleepState.Accelerated)
            {
                return;
            }

            _state = SleepState.Counting;
            if (isPaused)
            {
                return;
            }

            _elapsedSeconds += unscaledDeltaTime;
            if (_elapsedSeconds < ActivationDelaySeconds)
            {
                return;
            }

            var restoreSpeed = gateway.UserSelectedSpeed;
            gateway.SetTemporarySpeed(AcceleratedSpeed);
            _restoreSpeed = restoreSpeed;
            _state = SleepState.Accelerated;
        }

        public void NotifyUserSpeedChanged()
        {
            if (_state != SleepState.Accelerated)
            {
                return;
            }

            _state = SleepState.Suppressed;
            _elapsedSeconds = 0f;
            _restoreSpeed = 0f;
        }

        public void Reset(IGameSpeedGateway gateway)
        {
            if (_state == SleepState.Accelerated)
            {
                gateway.SetTemporarySpeed(_restoreSpeed);
            }

            Clear();
        }

        private void LeaveBed(IGameSpeedGateway gateway)
        {
            if (_state == SleepState.Accelerated)
            {
                gateway.SetTemporarySpeed(_restoreSpeed);
            }

            Clear();
        }

        private void Clear()
        {
            _state = SleepState.Idle;
            _elapsedSeconds = 0f;
            _restoreSpeed = 0f;
        }

        private enum SleepState
        {
            Idle,
            Counting,
            Accelerated,
            Suppressed
        }
    }
}
