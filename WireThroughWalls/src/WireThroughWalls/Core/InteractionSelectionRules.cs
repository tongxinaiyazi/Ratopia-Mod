namespace WireThroughWalls.Core
{
    internal static class InteractionSelectionRules
    {
        internal static T PreferSelectedTarget<T>(T selectedTarget, T lastEnteredTarget)
            where T : class
        {
            return selectedTarget ?? lastEnteredTarget;
        }
    }
}
