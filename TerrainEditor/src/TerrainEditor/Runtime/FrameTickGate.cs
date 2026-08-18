namespace TerrainEditor.Runtime
{
    internal sealed class FrameTickGate
    {
        private int _lastFrame = int.MinValue;

        public bool TryEnter(int frame)
        {
            if (_lastFrame == frame)
            {
                return false;
            }

            _lastFrame = frame;
            return true;
        }
    }
}
