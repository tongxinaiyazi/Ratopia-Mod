using System;
using HarmonyLib;
using StrongerWorkDistance.Runtime;

namespace StrongerWorkDistance.Patches
{
    [HarmonyPatch(typeof(SystemMgr), "Awake")]
    internal static class SystemMgrAwakePatch
    {
        private static void Postfix(SystemMgr __instance)
        {
            try
            {
                WorkAreaRuntime.Apply(__instance);
            }
            catch (Exception exception)
            {
                Plugin.LogRuntimeError($"工作距离应用失败，已保留原始站位：{exception}");
            }
        }
    }
}
