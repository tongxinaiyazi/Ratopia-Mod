using System;

namespace SuperBow.Core
{
    public static class TooltipRules
    {
        private const float MarkerTolerance = 0.0001f;

        public const string BleedText = "流血";

        public static bool IsBleedMarker(int abilityId, float value)
        {
            return abilityId == SuperBowConstants.BloodDrainAbilityId &&
                   Math.Abs(value - SuperBowConstants.BleedMarkerValue) <= MarkerTolerance;
        }
    }
}
