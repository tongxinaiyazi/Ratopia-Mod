namespace StrongerWorkDistance.Core
{
    public readonly struct WorkOffset
    {
        public WorkOffset(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }
    }
}
