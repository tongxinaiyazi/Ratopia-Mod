namespace EquipmentReforgeSelector
{
    public static class RuntimeEligibility
    {
        public static bool ShouldShow(bool isRobot, bool isUpgrade, int buildType, int level)
        {
            return !isRobot && isUpgrade && buildType == 3 && (level == 1 || level == 2);
        }
    }
}
