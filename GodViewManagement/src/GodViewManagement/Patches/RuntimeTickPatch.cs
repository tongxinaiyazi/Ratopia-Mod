using HarmonyLib;

namespace GodViewManagement.Patches
{
    [HarmonyPatch(typeof(TileMgr), "Update")]
    internal static class RuntimeTickPatch
    {
        private static void Postfix(TileMgr __instance)
        {
            Plugin.TickFromTileManager(__instance);
        }
    }
}
