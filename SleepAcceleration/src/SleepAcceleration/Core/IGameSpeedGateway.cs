namespace SleepAcceleration.Core
{
    internal interface IGameSpeedGateway
    {
        float UserSelectedSpeed { get; }

        void SetTemporarySpeed(float speed);
    }
}
