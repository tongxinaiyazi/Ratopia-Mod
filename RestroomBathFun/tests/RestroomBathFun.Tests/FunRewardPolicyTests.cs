using RestroomBathFun.Core;
using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class FunRewardPolicyTests
    {
        [Fact]
        public void DefaultSettingsUseThePlannedRewards()
        {
            var settings = RewardSettings.Default;

            Assert.Equal(25f, settings.ToiletFunReward);
            Assert.Equal(30f, settings.BathsFunReward);
        }

        [Fact]
        public void CompletedToiletReturnsItsConfiguredReward()
        {
            var settings = new RewardSettings(17f, 42f);

            var actual = FunRewardPolicy.Resolve(FacilityKind.Toilet, false, settings);

            Assert.Equal(17f, actual);
        }

        [Fact]
        public void CompletedBathsReturnsItsConfiguredReward()
        {
            var settings = new RewardSettings(17f, 42f);

            var actual = FunRewardPolicy.Resolve(FacilityKind.Baths, false, settings);

            Assert.Equal(42f, actual);
        }

        [Fact]
        public void AbortedSupportedServicesReturnNoReward()
        {
            var toilet = FunRewardPolicy.Resolve(
                FacilityKind.Toilet,
                true,
                RewardSettings.Default);
            var baths = FunRewardPolicy.Resolve(
                FacilityKind.Baths,
                true,
                RewardSettings.Default);

            Assert.Equal(0f, toilet);
            Assert.Equal(0f, baths);
        }

        [Fact]
        public void UnsupportedFacilityReturnsNoReward()
        {
            var actual = FunRewardPolicy.Resolve(
                FacilityKind.Unsupported,
                false,
                RewardSettings.Default);

            Assert.Equal(0f, actual);
        }
    }
}
