using System;

namespace GodViewManagement
{
    internal struct PanDelta
    {
        public PanDelta(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    internal static class CameraPanMath
    {
        public static PanDelta CalculateDelta(float x, float y, float speed, float deltaTime)
        {
            var magnitude = (float)Math.Sqrt(x * x + y * y);
            if (magnitude <= 0.0001f || speed <= 0f || deltaTime <= 0f)
            {
                return new PanDelta(0f, 0f);
            }

            var distance = speed * deltaTime;
            return new PanDelta(x / magnitude * distance, y / magnitude * distance);
        }
    }
}
