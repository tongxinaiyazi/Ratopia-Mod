using System.Collections.Generic;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class SkinRepairPolicyTests
    {
        [Fact]
        public void CompleteAppearanceRequiresBodyFaceHairAndDress()
        {
            Assert.True(SkinRepairPolicy.HasRequiredAppearance(CompleteAppearance()));
        }

        [Theory]
        [InlineData("Skin")]
        [InlineData("Face")]
        [InlineData("Hair")]
        [InlineData("Dress")]
        public void EmptyRequiredCategoryMakesAppearanceIncomplete(string category)
        {
            var appearance = CompleteAppearance();
            appearance[category] = "";

            Assert.False(SkinRepairPolicy.HasRequiredAppearance(appearance));
            Assert.Contains(category, SkinRepairPolicy.MissingRequiredCategories(appearance));
        }

        [Fact]
        public void EmptyOptionalCategoriesRemainValid()
        {
            var appearance = CompleteAppearance();
            appearance["Glasses"] = "";
            appearance["Hat"] = "";
            appearance["Makeup"] = "";

            Assert.True(SkinRepairPolicy.HasRequiredAppearance(appearance));
        }

        [Fact]
        public void RecoveryUsesSnapshotOnlyWhenItIsComplete()
        {
            Assert.Equal(SkinRecoveryKind.Snapshot, SkinRepairPolicy.SelectRecovery(CompleteAppearance()));
            Assert.Equal(SkinRecoveryKind.Default, SkinRepairPolicy.SelectRecovery(new Dictionary<string, string>()));
        }

        private static Dictionary<string, string> CompleteAppearance()
        {
            return new Dictionary<string, string>
            {
                ["Skin"] = "White",
                ["Face"] = "Face_1",
                ["Hair"] = "Hair_1",
                ["Dress"] = "Dress_1"
            };
        }
    }
}
