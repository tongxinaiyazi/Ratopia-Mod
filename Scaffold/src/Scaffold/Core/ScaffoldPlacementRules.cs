namespace ScaffoldMod.Core
{
    internal enum ScaffoldCellKind
    {
        Empty,
        Water,
        Plant,
        Building,
        Door,
        Barricade,
        SolidTerrain,
        Mineral,
        Ladder
    }

    internal static class ScaffoldPlacementRules
    {
        internal static bool CanPlace(ScaffoldCellKind kind, bool alreadyHasScaffold)
        {
            if (alreadyHasScaffold)
            {
                return false;
            }

            return kind != ScaffoldCellKind.SolidTerrain &&
                   kind != ScaffoldCellKind.Mineral &&
                   kind != ScaffoldCellKind.Ladder;
        }
    }
}
