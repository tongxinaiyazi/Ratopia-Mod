namespace SuperBow.Core
{
    public static class SplashRules
    {
        public static bool ShouldDamage(
            bool isPrimary,
            bool isEnemy,
            bool isAlive,
            float centerX,
            float centerY,
            float targetX,
            float targetY)
        {
            if (isPrimary || !isEnemy || !isAlive)
            {
                return false;
            }

            var deltaX = targetX - centerX;
            var deltaY = targetY - centerY;
            return deltaX * deltaX + deltaY * deltaY <=
                   SuperBowConstants.SplashRadius * SuperBowConstants.SplashRadius;
        }

        public static float CalculateDamage(float directDamage)
        {
            return directDamage * SuperBowConstants.SplashDamageMultiplier;
        }
    }
}
