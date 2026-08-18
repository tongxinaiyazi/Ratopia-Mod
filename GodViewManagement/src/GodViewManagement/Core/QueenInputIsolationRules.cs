namespace GodViewManagement
{
    internal static class QueenInputIsolationRules
    {
        public static bool ShouldSuppress(bool modeEnabled, bool inQueenUpdate, int hotKeyValue)
        {
            if (!modeEnabled || !inQueenUpdate)
            {
                return false;
            }

            return (hotKeyValue >= 0 && hotKeyValue <= 7)
                || hotKeyValue == 20
                || (hotKeyValue >= 22 && hotKeyValue <= 25)
                || (hotKeyValue >= 27 && hotKeyValue <= 29);
        }
    }
}
