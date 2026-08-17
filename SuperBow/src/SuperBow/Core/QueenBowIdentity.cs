using System;

namespace SuperBow.Core
{
    public static class QueenBowIdentity
    {
        public static bool IsMatch(int index, int type, string name)
        {
            return index == SuperBowConstants.QueenBowIndex &&
                   type == SuperBowConstants.QueenBowType &&
                   string.Equals(
                       name,
                       SuperBowConstants.QueenBowName,
                       StringComparison.Ordinal);
        }
    }
}
