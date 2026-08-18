namespace GodViewManagement
{
    internal sealed class RuntimeTickGate
    {
        private int _lastFrame = int.MinValue;

        public bool TryEnter(int frameCount)
        {
            if (_lastFrame == frameCount)
            {
                return false;
            }

            _lastFrame = frameCount;
            return true;
        }
    }
}
