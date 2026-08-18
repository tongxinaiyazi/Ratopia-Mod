namespace EquipmentReforgeSelector
{
    internal static class CandidateShortcut
    {
        public static bool TryResolveDigit(int digit, int candidateCount, out int candidateIndex)
        {
            candidateIndex = digit - 1;
            return digit >= 1 && digit <= 9 && candidateCount > 0 && candidateIndex < candidateCount;
        }
    }
}
