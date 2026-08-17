using System;

namespace ScaffoldMod.Core
{
    internal readonly struct ScaffoldRecord : IEquatable<ScaffoldRecord>
    {
        internal ScaffoldRecord(int x, int y, int expiryMinute, int underlyingNodeType)
        {
            X = x;
            Y = y;
            ExpiryMinute = expiryMinute;
            UnderlyingNodeType = underlyingNodeType;
        }

        internal int X { get; }

        internal int Y { get; }

        internal int ExpiryMinute { get; }

        internal int UnderlyingNodeType { get; }

        public bool Equals(ScaffoldRecord other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   ExpiryMinute == other.ExpiryMinute &&
                   UnderlyingNodeType == other.UnderlyingNodeType;
        }

        public override bool Equals(object obj)
        {
            return obj is ScaffoldRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = X;
                hash = hash * 397 ^ Y;
                hash = hash * 397 ^ ExpiryMinute;
                hash = hash * 397 ^ UnderlyingNodeType;
                return hash;
            }
        }

        public static bool operator ==(ScaffoldRecord left, ScaffoldRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScaffoldRecord left, ScaffoldRecord right)
        {
            return !left.Equals(right);
        }
    }
}
