using System;
using HarmonyLib;

namespace RatopiaMod.YunQing.All.Patches
{
    [HarmonyPatch(typeof(Fish), "DrownCheck")]
    internal static class FishDrownCheckPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Fish __instance)
        {
            try
            {
                Plugin.LogPatchInvocationOnce("fish-drown-invoked", "Fish.DrownCheck");
                if (!Plugin.FishFeatureEnabled)
                {
                    return true;
                }

                __instance.BeAttacked(-5f);
                return false;
            }
            catch (Exception error)
            {
                Plugin.LogPatchErrorOnce("fish-drown", "Fish.DrownCheck 补丁执行失败", error);
                return true;
            }
        }
    }
}
