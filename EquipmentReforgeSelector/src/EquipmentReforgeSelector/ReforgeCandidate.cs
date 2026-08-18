using System;

namespace EquipmentReforgeSelector
{
    public readonly struct ReforgeCandidate : IEquatable<ReforgeCandidate>
    {
        public ReforgeCandidate(int abilityId, float value)
        {
            AbilityId = abilityId;
            Value = value;
        }

        public int AbilityId { get; }

        public float Value { get; }

        public bool Equals(ReforgeCandidate other)
        {
            return AbilityId == other.AbilityId && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ReforgeCandidate && Equals((ReforgeCandidate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (AbilityId * 397) ^ Value.GetHashCode();
            }
        }

        public static bool operator ==(ReforgeCandidate left, ReforgeCandidate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReforgeCandidate left, ReforgeCandidate right)
        {
            return !left.Equals(right);
        }
    }
}
