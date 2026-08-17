using System;

namespace SuperBow.Core
{
    public static class DamageDisplayRules
    {
        public static int RoundForDisplay(float exactDamage)
        {
            if (exactDamage <= 0f)
            {
                return 0;
            }

            return Math.Max(
                1,
                (int)Math.Floor(exactDamage + 0.5f));
        }
    }
}
