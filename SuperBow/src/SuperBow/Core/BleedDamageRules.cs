namespace SuperBow.Core
{
    public static class BleedDamageRules
    {
        public static float CalculateExact(float maxHealth, float fraction)
        {
            if (maxHealth <= 0f || fraction <= 0f)
            {
                return 0f;
            }

            return maxHealth * fraction;
        }

        public static int CalculateApplied(float maxHealth, float fraction)
        {
            return DamageDisplayRules.RoundForDisplay(
                CalculateExact(maxHealth, fraction));
        }
    }
}
