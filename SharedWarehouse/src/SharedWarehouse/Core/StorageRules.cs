using System.Globalization;

namespace SharedWarehouse.Core
{
    internal static class StorageRules
    {
        public const int NormalStorage = 100;
        public const int MiniStorage = 181;

        public static bool IsTargetBuilding(int buildingName)
        {
            return buildingName == NormalStorage || buildingName == MiniStorage;
        }

        public static string FormatCapacity(int materialTypeCount)
        {
            return materialTypeCount.ToString(CultureInfo.InvariantCulture) + "/∞";
        }
    }
}
