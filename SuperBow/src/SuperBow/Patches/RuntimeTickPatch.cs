using System;
using HarmonyLib;
using SuperBow.Runtime;
using UnityEngine;

namespace SuperBow.Patches
{
    [HarmonyPatch(typeof(T_Queen), "Update")]
    internal static class RuntimeTickPatch
    {
        private static void Postfix()
        {
            try
            {
                var manager = GameMgr.Instance;
                RuntimeCatalog.TryApplySafely(manager != null ? manager._DB_Mgr : null);
            }
            catch (Exception exception)
            {
                RuntimeCatalog.ReportLookupFailure(exception);
            }

            CombatRuntime.TickSafely(Time.time);
        }
    }
}
