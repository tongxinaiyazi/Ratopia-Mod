using HarmonyLib;

namespace TerrainEditor.Patches
{
    [HarmonyPatch(typeof(LoadingSceneMgr), "Start")]
    internal static class LoadingSceneStartPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            Plugin.PrepareForSceneChange();
        }
    }
}
