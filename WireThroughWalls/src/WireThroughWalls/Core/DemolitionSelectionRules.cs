namespace WireThroughWalls.Core
{
    internal enum DemolitionTargetPreference
    {
        Original,
        Foreground,
        Wire
    }

    internal static class DemolitionSelectionRules
    {
        internal static DemolitionTargetPreference GetPreference(
            bool hasForeground,
            bool hasWire,
            bool altPressed)
        {
            if (hasForeground && (!altPressed || !hasWire))
            {
                return DemolitionTargetPreference.Foreground;
            }

            if (hasWire)
            {
                return DemolitionTargetPreference.Wire;
            }

            return DemolitionTargetPreference.Original;
        }
    }
}
