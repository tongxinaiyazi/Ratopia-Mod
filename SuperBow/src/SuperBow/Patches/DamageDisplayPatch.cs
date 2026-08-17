using HarmonyLib;
using SuperBow.Runtime;
using UnityEngine;

namespace SuperBow.Patches
{
    [HarmonyPatch(
        typeof(DmgEffect),
        "SetDmgEffect",
        new[]
        {
            typeof(int),
            typeof(Vector3),
            typeof(Transform),
            typeof(bool),
            typeof(int)
        })]
    internal static class DamageDisplayPatch
    {
        private static void Prefix(ref int __0)
        {
            if (DamageDisplayRuntime.TryGetOverride(out var displayDamage))
            {
                __0 = displayDamage;
            }
        }
    }
}
