using HarmonyLib;
using SharedWarehouse.Core;
using TMPro;

namespace SharedWarehouse.Patches
{
    [HarmonyPatch(typeof(BI_StorageUI), nameof(BI_StorageUI.InfoUpdate))]
    internal static class StorageUiPatch
    {
        private static void Postfix(BI_StorageUI __instance, TextMeshProUGUI ___Txt_Num)
        {
            var storage = __instance?.m_Building as Building_Storage;
            if (___Txt_Num == null || !StorageInventoryCoordinator.IsTarget(storage))
            {
                return;
            }

            ___Txt_Num.text = StorageRules.FormatCapacity(storage.List_TileObj?.Count ?? 0);
        }
    }
}
