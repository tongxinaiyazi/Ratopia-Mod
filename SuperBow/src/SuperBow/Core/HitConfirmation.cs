namespace SuperBow.Core
{
    public static class HitConfirmation
    {
        public static bool DidTakeDamage(float before, float after)
        {
            return before > 0f && after < before;
        }
    }
}
