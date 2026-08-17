namespace WireThroughWalls.Core
{
    internal static class OverlayRules
    {
        internal static bool CanBlueprintsShare(bool candidateIsWire, bool existingIsWire)
        {
            return candidateIsWire != existingIsWire;
        }

        internal static bool ShouldHideBuildType(bool candidateIsWire, bool hasForegroundOwner)
        {
            return candidateIsWire && hasForegroundOwner;
        }

        internal static bool RequiresCoordination(bool candidateIsWire, bool hasExistingWire)
        {
            return candidateIsWire || hasExistingWire;
        }

        internal static bool ShouldMaskCompletedWiresDuringCompletion(
            bool candidateIsWire,
            int candidateAbility)
        {
            return !candidateIsWire && candidateAbility == 2;
        }
    }
}
