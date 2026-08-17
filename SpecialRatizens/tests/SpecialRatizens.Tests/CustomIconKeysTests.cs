using System;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class CustomIconKeysTests
    {
        [Fact]
        public void TraitKeyUsesAnIsolatedStableNamespace()
        {
            Assert.Equal("SpecialRatizens.Icon.HT_WQX", CustomIconKeys.ForTrait("HT_WQX"));
        }

        [Fact]
        public void CharacterIndexKeyMatchesRatopiaUiConvention()
        {
            Assert.Equal("Icon_Char153", CustomIconKeys.ForCharacterIndex(153));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TraitKeyRejectsMissingNames(string value)
        {
            Assert.Throws<ArgumentException>(() => CustomIconKeys.ForTrait(value));
        }

        [Fact]
        public void CharacterIndexKeyRejectsNegativeIndexes()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CustomIconKeys.ForCharacterIndex(-1));
        }
    }
}
