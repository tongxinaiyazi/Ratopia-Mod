using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class ReleaseDataContractTests
    {
        [Fact]
        public void ShippedCatalogContainsTwelveRatizensAndTwentyFourTraitsAndIcons()
        {
            var root = GetProjectRoot();
            var data = Path.Combine(root, "Data");

            var catalog = SpecialDataCatalog.Load(
                Path.Combine(data, "CustomSpecialUnit.csv"),
                Path.Combine(data, "CustomCharInfo.csv"),
                Path.Combine(data, "Icon"));

            Assert.Equal(12, catalog.Ratizens.Count);
            Assert.Equal(24, catalog.Traits.Count);
            Assert.Equal(24, catalog.Ratizens.SelectMany(item => new[] { item.Icon1, item.Icon2 }).Distinct().Count());
        }

        [Fact]
        public void EveryShippedTraitHasExactlyOneRatizenOwner()
        {
            var catalog = LoadShippedCatalog();
            var references = catalog.Ratizens
                .SelectMany(item => new[] { item.Trait1, item.Trait2 })
                .GroupBy(item => item, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

            Assert.Equal(
                catalog.Traits.Select(item => item.Name).OrderBy(item => item, StringComparer.Ordinal),
                references.Keys.OrderBy(item => item, StringComparer.Ordinal));
            Assert.All(references, item => Assert.Equal(1, item.Value));
        }

        [Fact]
        public void EveryShippedRatizenHasTheRequiredVisibleBodyParts()
        {
            var catalog = LoadShippedCatalog();

            Assert.All(catalog.Ratizens, ratizen =>
            {
                Assert.False(string.IsNullOrWhiteSpace(ratizen.Skin), ratizen.Name + " 缺少 Skin");
                Assert.False(string.IsNullOrWhiteSpace(ratizen.Face), ratizen.Name + " 缺少 Face");
                Assert.False(string.IsNullOrWhiteSpace(ratizen.Hair), ratizen.Name + " 缺少 Hair");
                Assert.False(string.IsNullOrWhiteSpace(ratizen.Dress), ratizen.Name + " 缺少 Dress");
            });
        }

        [Fact]
        public void ShippedFormulaDivisorsAreNonZero()
        {
            var traits = LoadShippedCatalog().Traits.ToDictionary(item => item.Name, StringComparer.Ordinal);
            var effectADivisors = new[]
            {
                "NaiNai_Benevolence", "SY_KCL", "SY_QL", "YF_YJQ", "WH_SZ",
                "LLJ_KYSS", "PKQ_SWFT", "AMJ7_LZDW"
            };
            var effectBDivisors = new[] { "PKQ_DQCD", "AMJ7_LZJX" };

            Assert.All(effectADivisors, name => Assert.NotEqual(0f, traits[name].EffectValueA));
            Assert.All(effectBDivisors, name => Assert.NotEqual(0f, traits[name].EffectValueB));
        }

        private static SpecialDataCatalog LoadShippedCatalog()
        {
            var data = Path.Combine(GetProjectRoot(), "Data");
            return SpecialDataCatalog.Load(
                Path.Combine(data, "CustomSpecialUnit.csv"),
                Path.Combine(data, "CustomCharInfo.csv"),
                Path.Combine(data, "Icon"));
        }

        private static string GetProjectRoot()
        {
            return typeof(ReleaseDataContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
