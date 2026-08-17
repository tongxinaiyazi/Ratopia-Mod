using System;
using HarmonyLib;

namespace RatopiaMod.YunQing.All.Patches
{
    [HarmonyPatch(typeof(Monkfish), "DrownCheck")]
    internal static class MonkfishDrownCheckPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Monkfish __instance)
        {
            try
            {
                Plugin.LogPatchInvocationOnce("monkfish-drown-invoked", "Monkfish.DrownCheck");
                if (!Plugin.FishFeatureEnabled)
                {
                    return true;
                }

                __instance.BeAttacked(-5f);
                return false;
            }
            catch (Exception error)
            {
                Plugin.LogPatchErrorOnce("monkfish-drown", "Monkfish.DrownCheck 补丁执行失败", error);
                return true;
            }
        }
    }
}
