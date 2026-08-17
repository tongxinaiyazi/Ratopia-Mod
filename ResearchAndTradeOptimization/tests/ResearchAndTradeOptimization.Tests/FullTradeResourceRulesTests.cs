using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class FullTradeResourceRulesTests
    {
        [Fact]
        public void BuildIncludesEveryConfiguredResourceInStableOrder()
        {
            var result = Build(
                new[] { 1, 4 },
                new[]
                {
                    new[] { 101, 102, 103 },
                    new[] { 201, 202 }
                },
                Array.Empty<int>());

            Assert.Equal(
                new[] { "1:101", "1:102", "1:103", "4:201", "4:202" },
                result);
        }

        [Fact]
        public void BuildSkipsIgnoredResourcesAndKeepsFirstDuplicateOccurrence()
        {
            var result = Build(
                new[] { 2, 6, 9 },
                new[]
                {
                    new[] { 10, 11, 12 },
                    Array.Empty<int>(),
                    new[] { 12, 13, 10, 14 }
                },
                new[] { 11, 14 });

            Assert.Equal(new[] { "2:10", "2:12", "9:13" }, result);
        }

        [Fact]
        public void BuildDoesNotAcceptOrApplyVanillaPickCounts()
        {
            var method = GetRulesType().GetMethod(
                "Build",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(method);
            Assert.Equal(
                new[] { typeof(int[]), typeof(int[][]), typeof(int[]) },
                method.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(
                new[] { "3:1", "3:2", "3:3", "3:4" },
                Build(new[] { 3 }, new[] { new[] { 1, 2, 3, 4 } }, Array.Empty<int>()));
        }

        [Fact]
        public void SharedGroupIsPartitionedAcrossDirectionsByEveryProsperityWeight()
        {
            var resources = Enumerable.Range(1, 18).ToArray();
            var result = BuildBothDirections(
                new[]
                {
                    Bucket("Exception_A", 3, 4, resources),
                    Bucket("Exception_A", 6, 2, resources),
                    Bucket("Exception_A", 8, 4, resources)
                },
                new[]
                {
                    Bucket("Exception_A", 3, 2, resources),
                    Bucket("Exception_A", 6, 3, resources),
                    Bucket("Exception_A", 8, 3, resources)
                },
                Array.Empty<int>());

            Assert.Empty(result.countryToHometown.Intersect(result.hometownToCountry));
            Assert.Equal(resources, result.countryToHometown
                .Concat(result.hometownToCountry)
                .OrderBy(value => value));
            Assert.Equal(10, result.countryToHometown.Length);
            Assert.Equal(8, result.hometownToCountry.Length);
            Assert.Equal(new[] { 3, 6, 8 }, result.countryProsperities.Distinct());
            Assert.Equal(new[] { 3, 6, 8 }, result.hometownProsperities.Distinct());
        }

        [Fact]
        public void IndependentGroupsKeepTheirDirectionAndCrossGroupDuplicateUsesLowerProsperity()
        {
            var result = BuildBothDirections(
                new[]
                {
                    Bucket("CountryOnly", 7, 1, new[] { 10, 11, 30 })
                },
                new[]
                {
                    Bucket("HometownOnly", 2, 1, new[] { 20, 21, 30, 99 })
                },
                new[] { 99 });

            Assert.Equal(new[] { 10, 11 }, result.countryToHometown);
            Assert.Equal(new[] { 20, 21, 30 }, result.hometownToCountry);
            Assert.Empty(result.countryToHometown.Intersect(result.hometownToCountry));
        }

        [Fact]
        public void OnlyLocalBucketsAllowCompletePoolExpansion()
        {
            Assert.True(CanExpandAll(
                new[] { Bucket("Exception_A", 1, 1, new[] { 10 }, isGlobal: false) },
                new[] { Bucket("Local", 1, 1, new[] { 20 }, isGlobal: false) }));
        }

        [Fact]
        public void AnyGlobalBucketKeepsTheEntireCountryOnVanillaSelection()
        {
            Assert.False(CanExpandAll(
                new[] { Bucket("Local", 1, 1, new[] { 10 }, isGlobal: false) },
                new[] { Bucket("AnyFutureGlobalGroupName", 1, 1, new[] { 20 }, isGlobal: true) }));
        }

        [Fact]
        public void OversizedSavedSelectionForGlobalPoolRequiresVanillaRepair()
        {
            Assert.True(NeedsVanillaRepair(
                new[]
                {
                    Bucket("Exception_A", 3, 4, Enumerable.Range(1, 119).ToArray(), isGlobal: true),
                    Bucket("Exception_A", 6, 6, Enumerable.Range(1, 119).ToArray(), isGlobal: true)
                },
                currentCountryToHometownCount: 70,
                new[]
                {
                    Bucket("Exception_A", 3, 4, Enumerable.Range(1, 119).ToArray(), isGlobal: true),
                    Bucket("Exception_A", 6, 5, Enumerable.Range(1, 119).ToArray(), isGlobal: true)
                },
                currentHometownToCountryCount: 49));
        }

        [Fact]
        public void NormalSavedSelectionForGlobalPoolIsPreserved()
        {
            Assert.False(NeedsVanillaRepair(
                new[] { Bucket("Exception_A", 3, 10, Enumerable.Range(1, 119).ToArray(), isGlobal: true) },
                currentCountryToHometownCount: 10,
                new[] { Bucket("Exception_A", 3, 9, Enumerable.Range(1, 119).ToArray(), isGlobal: true) },
                currentHometownToCountryCount: 8));
        }

        [Fact]
        public void ExpandedLocalPoolNeverUsesVanillaRepairPath()
        {
            Assert.False(NeedsVanillaRepair(
                new[] { Bucket("Local_A", 3, 2, Enumerable.Range(1, 14).ToArray()) },
                currentCountryToHometownCount: 14,
                new[] { Bucket("Local_B", 3, 2, Enumerable.Range(20, 12).ToArray()) },
                currentHometownToCountryCount: 12));
        }

        private static string[] Build(
            int[] prosperityValues,
            int[][] resourceGroups,
            int[] ignoredResources)
        {
            var method = GetRulesType().GetMethod(
                "Build",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            var result = (Array)method.Invoke(
                null,
                new object[] { prosperityValues, resourceGroups, ignoredResources });

            return result.Cast<object>()
                .Select(item =>
                {
                    var type = item.GetType();
                    var key = type.GetProperty("Key").GetValue(item);
                    var value = type.GetProperty("Value").GetValue(item);
                    return $"{key}:{value}";
                })
                .ToArray();
        }

        private static Type GetRulesType()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "ResearchAndTradeOptimization.dll");
            Assert.True(File.Exists(path), $"Plugin assembly not found: {path}");
            return Assembly.LoadFrom(path).GetType(
                "ResearchAndTradeOptimization.Core.FullTradeResourceRules",
                throwOnError: true);
        }

        private static object Bucket(
            string groupKey,
            int prosperity,
            int weight,
            int[] resources,
            bool isGlobal = false)
        {
            var bucketType = Assembly.LoadFrom(Path.Combine(
                    AppContext.BaseDirectory,
                    "ResearchAndTradeOptimization.dll"))
                .GetType(
                    "ResearchAndTradeOptimization.Core.TradeResourceBucket",
                    throwOnError: false);
            Assert.NotNull(bucketType);
            return Activator.CreateInstance(
                bucketType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { groupKey, prosperity, weight, resources, isGlobal },
                null);
        }

        private static bool CanExpandAll(
            object[] countryToHometown,
            object[] hometownToCountry)
        {
            var rulesType = GetRulesType();
            var bucketType = rulesType.Assembly.GetType(
                "ResearchAndTradeOptimization.Core.TradeResourceBucket",
                throwOnError: true);
            var bucketArrayType = bucketType.MakeArrayType();
            var first = Array.CreateInstance(bucketType, countryToHometown.Length);
            var second = Array.CreateInstance(bucketType, hometownToCountry.Length);
            Array.Copy(countryToHometown, first, countryToHometown.Length);
            Array.Copy(hometownToCountry, second, hometownToCountry.Length);

            var method = rulesType.GetMethod(
                "CanExpandAll",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { bucketArrayType, bucketArrayType },
                null);
            Assert.NotNull(method);
            return (bool)method.Invoke(null, new object[] { first, second });
        }

        private static bool NeedsVanillaRepair(
            object[] countryToHometown,
            int currentCountryToHometownCount,
            object[] hometownToCountry,
            int currentHometownToCountryCount)
        {
            var rulesType = GetRulesType();
            var bucketType = rulesType.Assembly.GetType(
                "ResearchAndTradeOptimization.Core.TradeResourceBucket",
                throwOnError: true);
            var bucketArrayType = bucketType.MakeArrayType();
            var first = Array.CreateInstance(bucketType, countryToHometown.Length);
            var second = Array.CreateInstance(bucketType, hometownToCountry.Length);
            Array.Copy(countryToHometown, first, countryToHometown.Length);
            Array.Copy(hometownToCountry, second, hometownToCountry.Length);

            var method = rulesType.GetMethod(
                "NeedsVanillaRepair",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { bucketArrayType, typeof(int), bucketArrayType, typeof(int) },
                null);
            Assert.NotNull(method);
            return (bool)method.Invoke(
                null,
                new object[]
                {
                    first,
                    currentCountryToHometownCount,
                    second,
                    currentHometownToCountryCount
                });
        }

        private static (
            int[] countryToHometown,
            int[] hometownToCountry,
            int[] countryProsperities,
            int[] hometownProsperities) BuildBothDirections(
                object[] countryToHometown,
                object[] hometownToCountry,
                int[] ignoredResources)
        {
            var rulesType = GetRulesType();
            var bucketType = rulesType.Assembly.GetType(
                "ResearchAndTradeOptimization.Core.TradeResourceBucket",
                throwOnError: false);
            Assert.NotNull(bucketType);
            var bucketArrayType = bucketType.MakeArrayType();
            var first = Array.CreateInstance(bucketType, countryToHometown.Length);
            var second = Array.CreateInstance(bucketType, hometownToCountry.Length);
            Array.Copy(countryToHometown, first, countryToHometown.Length);
            Array.Copy(hometownToCountry, second, hometownToCountry.Length);

            var method = rulesType.GetMethod(
                "BuildBothDirections",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { bucketArrayType, bucketArrayType, typeof(int[]) },
                null);
            Assert.NotNull(method);
            var result = method.Invoke(null, new object[] { first, second, ignoredResources });
            Assert.NotNull(result);
            var resultType = result.GetType();
            var country = (Array)resultType.GetProperty("CountryToHometown").GetValue(result);
            var hometown = (Array)resultType.GetProperty("HometownToCountry").GetValue(result);
            return (
                Values(country),
                Values(hometown),
                Prosperities(country),
                Prosperities(hometown));
        }

        private static int[] Values(Array selections)
        {
            return selections.Cast<object>()
                .Select(item => Convert.ToInt32(item.GetType().GetProperty("Value").GetValue(item)))
                .ToArray();
        }

        private static int[] Prosperities(Array selections)
        {
            return selections.Cast<object>()
                .Select(item => Convert.ToInt32(item.GetType().GetProperty("Key").GetValue(item)))
                .ToArray();
        }
    }
}
