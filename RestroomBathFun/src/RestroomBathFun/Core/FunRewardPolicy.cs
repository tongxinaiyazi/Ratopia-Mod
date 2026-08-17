namespace RestroomBathFun.Core
{
    internal static class FunRewardPolicy
    {
        internal static float Resolve(
            FacilityKind facility,
            bool serviceAborted,
            RewardSettings settings)
        {
            if (serviceAborted)
            {
                return 0f;
            }

            switch (facility)
            {
                case FacilityKind.Toilet:
                    return settings.ToiletFunReward;
                case FacilityKind.Baths:
                    return settings.BathsFunReward;
                default:
                    return 0f;
            }
        }
    }
}
