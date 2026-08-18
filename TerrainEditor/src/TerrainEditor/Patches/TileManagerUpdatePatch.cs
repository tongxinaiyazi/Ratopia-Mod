using HarmonyLib;

namespace TerrainEditor.Patches
{
    [HarmonyPatch(typeof(TileMgr), "Update")]
    internal static class TileManagerUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(TileMgr __instance)
        {
            Plugin.TickFromTileManager(__instance);
        }
    }
}
