namespace RestroomBathFun.Core
{
    internal readonly struct RewardSettings
    {
        internal static RewardSettings Default => new RewardSettings(25f, 30f);

        internal RewardSettings(float toiletFunReward, float bathsFunReward)
        {
            ToiletFunReward = toiletFunReward;
            BathsFunReward = bathsFunReward;
        }

        internal float ToiletFunReward { get; }

        internal float BathsFunReward { get; }
    }
}
