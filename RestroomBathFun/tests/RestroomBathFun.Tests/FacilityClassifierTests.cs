using RestroomBathFun.Core;
using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class FacilityClassifierTests
    {
        [Fact]
        public void VanillaToiletIsSupported()
        {
            Assert.Equal(FacilityKind.Toilet, FacilityClassifier.Classify(110));
        }

        [Fact]
        public void BathsAreSupported()
        {
            Assert.Equal(FacilityKind.Baths, FacilityClassifier.Classify(114));
        }

        [Fact]
        public void ElectricToiletIsExplicitlyUnsupported()
        {
            Assert.Equal(FacilityKind.Unsupported, FacilityClassifier.Classify(308));
        }

        [Fact]
        public void UnknownBuildingIsUnsupported()
        {
            Assert.Equal(FacilityKind.Unsupported, FacilityClassifier.Classify(-1));
        }
    }
}
