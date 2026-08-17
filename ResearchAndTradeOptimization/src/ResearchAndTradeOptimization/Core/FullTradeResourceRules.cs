using System;
using System.Collections.Generic;

namespace ResearchAndTradeOptimization.Core
{
    internal readonly struct TradeResourceSelection
    {
        internal TradeResourceSelection(int key, int value)
        {
            Key = key;
            Value = value;
        }

        public int Key { get; }

        public int Value { get; }
    }

    internal readonly struct TradeResourceBucket
    {
        internal TradeResourceBucket(
            string groupKey,
            int prosperityValue,
            int weight,
            int[] resources,
            bool isGlobal)
        {
            GroupKey = groupKey ?? string.Empty;
            ProsperityValue = prosperityValue;
            Weight = weight;
            Resources = resources ?? Array.Empty<int>();
            IsGlobal = isGlobal;
        }

        internal string GroupKey { get; }

        internal int ProsperityValue { get; }

        internal int Weight { get; }

        internal int[] Resources { get; }

        internal bool IsGlobal { get; }
    }

    internal readonly struct TradeResourceDirectionResult
    {
        internal TradeResourceDirectionResult(
            TradeResourceSelection[] countryToHometown,
            TradeResourceSelection[] hometownToCountry)
        {
            CountryToHometown = countryToHometown ?? Array.Empty<TradeResourceSelection>();
            HometownToCountry = hometownToCountry ?? Array.Empty<TradeResourceSelection>();
        }

        public TradeResourceSelection[] CountryToHometown { get; }

        public TradeResourceSelection[] HometownToCountry { get; }
    }

    internal static class FullTradeResourceRules
    {
        internal static bool CanExpandAll(
            TradeResourceBucket[] countryToHometown,
            TradeResourceBucket[] hometownToCountry)
        {
            if (countryToHometown == null)
            {
                throw new ArgumentNullException(nameof(countryToHometown));
            }

            if (hometownToCountry == null)
            {
                throw new ArgumentNullException(nameof(hometownToCountry));
            }

            for (var index = 0; index < countryToHometown.Length; index++)
            {
                if (countryToHometown[index].IsGlobal)
                {
                    return false;
                }
            }

            for (var index = 0; index < hometownToCountry.Length; index++)
            {
                if (hometownToCountry[index].IsGlobal)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool NeedsVanillaRepair(
            TradeResourceBucket[] countryToHometown,
            int currentCountryToHometownCount,
            TradeResourceBucket[] hometownToCountry,
            int currentHometownToCountryCount)
        {
            if (countryToHometown == null)
            {
                throw new ArgumentNullException(nameof(countryToHometown));
            }

            if (hometownToCountry == null)
            {
                throw new ArgumentNullException(nameof(hometownToCountry));
            }

            if (CanExpandAll(countryToHometown, hometownToCountry))
            {
                return false;
            }

            return currentCountryToHometownCount > GetVanillaMaximum(countryToHometown) ||
                   currentHometownToCountryCount > GetVanillaMaximum(hometownToCountry);
        }

        internal static TradeResourceDirectionResult BuildBothDirections(
            TradeResourceBucket[] countryToHometown,
            TradeResourceBucket[] hometownToCountry,
            int[] ignoredResources)
        {
            if (countryToHometown == null)
            {
                throw new ArgumentNullException(nameof(countryToHometown));
            }

            if (hometownToCountry == null)
            {
                throw new ArgumentNullException(nameof(hometownToCountry));
            }

            var ignored = new HashSet<int>(ignoredResources ?? Array.Empty<int>());
            var groups = new List<GroupAllocation>();
            var groupsByKey = new Dictionary<string, GroupAllocation>(StringComparer.Ordinal);
            AddBuckets(groups, groupsByKey, countryToHometown, isCountryToHometown: true);
            AddBuckets(groups, groupsByKey, hometownToCountry, isCountryToHometown: false);

            var candidates = new List<ResourceCandidate>();
            var candidateOrder = 0;
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                var schedule = new List<BucketAllocation>();
                for (var bucketIndex = 0; bucketIndex < group.Buckets.Count; bucketIndex++)
                {
                    var bucket = group.Buckets[bucketIndex];
                    for (var weightIndex = 0; weightIndex < bucket.Bucket.Weight; weightIndex++)
                    {
                        schedule.Add(bucket);
                    }
                }

                if (schedule.Count == 0)
                {
                    continue;
                }

                var resources = new List<int>();
                var groupResources = new HashSet<int>();
                for (var bucketIndex = 0; bucketIndex < group.Buckets.Count; bucketIndex++)
                {
                    var bucketResources = group.Buckets[bucketIndex].Bucket.Resources;
                    for (var resourceIndex = 0; resourceIndex < bucketResources.Length; resourceIndex++)
                    {
                        var resource = bucketResources[resourceIndex];
                        if (!ignored.Contains(resource) && groupResources.Add(resource))
                        {
                            resources.Add(resource);
                        }
                    }
                }

                for (var resourceIndex = 0; resourceIndex < resources.Count; resourceIndex++)
                {
                    var owner = schedule[resourceIndex % schedule.Count];
                    candidates.Add(new ResourceCandidate(
                        resources[resourceIndex],
                        owner.Bucket.ProsperityValue,
                        owner.IsCountryToHometown,
                        candidateOrder++));
                }
            }

            var winners = new Dictionary<int, ResourceCandidate>();
            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (!winners.TryGetValue(candidate.Resource, out var current) ||
                    candidate.ProsperityValue < current.ProsperityValue ||
                    (candidate.ProsperityValue == current.ProsperityValue &&
                     candidate.Order < current.Order))
                {
                    winners[candidate.Resource] = candidate;
                }
            }

            var ordered = new List<ResourceCandidate>(winners.Values);
            ordered.Sort((left, right) =>
            {
                var prosperity = left.ProsperityValue.CompareTo(right.ProsperityValue);
                return prosperity != 0 ? prosperity : left.Order.CompareTo(right.Order);
            });

            var countryResult = new List<TradeResourceSelection>();
            var hometownResult = new List<TradeResourceSelection>();
            for (var index = 0; index < ordered.Count; index++)
            {
                var candidate = ordered[index];
                var selection = new TradeResourceSelection(
                    candidate.ProsperityValue,
                    candidate.Resource);
                if (candidate.IsCountryToHometown)
                {
                    countryResult.Add(selection);
                }
                else
                {
                    hometownResult.Add(selection);
                }
            }

            return new TradeResourceDirectionResult(
                countryResult.ToArray(),
                hometownResult.ToArray());
        }

        internal static TradeResourceSelection[] Build(
            int[] prosperityValues,
            int[][] resourceGroups,
            int[] ignoredResources)
        {
            if (prosperityValues == null)
            {
                throw new ArgumentNullException(nameof(prosperityValues));
            }

            if (resourceGroups == null)
            {
                throw new ArgumentNullException(nameof(resourceGroups));
            }

            if (prosperityValues.Length != resourceGroups.Length)
            {
                throw new ArgumentException("繁荣度数组与资源组数组长度必须一致。");
            }

            var ignored = new HashSet<int>(ignoredResources ?? Array.Empty<int>());
            var seen = new HashSet<int>();
            var result = new List<TradeResourceSelection>();
            for (var groupIndex = 0; groupIndex < resourceGroups.Length; groupIndex++)
            {
                var resources = resourceGroups[groupIndex] ?? Array.Empty<int>();
                for (var resourceIndex = 0; resourceIndex < resources.Length; resourceIndex++)
                {
                    var resource = resources[resourceIndex];
                    if (ignored.Contains(resource) || !seen.Add(resource))
                    {
                        continue;
                    }

                    result.Add(new TradeResourceSelection(
                        prosperityValues[groupIndex],
                        resource));
                }
            }

            return result.ToArray();
        }

        private static void AddBuckets(
            ICollection<GroupAllocation> groups,
            IDictionary<string, GroupAllocation> groupsByKey,
            IEnumerable<TradeResourceBucket> buckets,
            bool isCountryToHometown)
        {
            foreach (var bucket in buckets)
            {
                if (bucket.Weight <= 0 || string.IsNullOrEmpty(bucket.GroupKey))
                {
                    continue;
                }

                if (!groupsByKey.TryGetValue(bucket.GroupKey, out var group))
                {
                    group = new GroupAllocation();
                    groupsByKey.Add(bucket.GroupKey, group);
                    groups.Add(group);
                }

                group.Buckets.Add(new BucketAllocation(bucket, isCountryToHometown));
            }
        }

        private static int GetVanillaMaximum(IEnumerable<TradeResourceBucket> buckets)
        {
            var total = 0;
            foreach (var bucket in buckets)
            {
                if (bucket.Weight <= 0)
                {
                    continue;
                }

                if (total > int.MaxValue - bucket.Weight)
                {
                    return int.MaxValue;
                }

                total += bucket.Weight;
            }

            return total;
        }

        private sealed class GroupAllocation
        {
            internal List<BucketAllocation> Buckets { get; } =
                new List<BucketAllocation>();
        }

        private readonly struct BucketAllocation
        {
            internal BucketAllocation(
                TradeResourceBucket bucket,
                bool isCountryToHometown)
            {
                Bucket = bucket;
                IsCountryToHometown = isCountryToHometown;
            }

            internal TradeResourceBucket Bucket { get; }

            internal bool IsCountryToHometown { get; }
        }

        private readonly struct ResourceCandidate
        {
            internal ResourceCandidate(
                int resource,
                int prosperityValue,
                bool isCountryToHometown,
                int order)
            {
                Resource = resource;
                ProsperityValue = prosperityValue;
                IsCountryToHometown = isCountryToHometown;
                Order = order;
            }

            internal int Resource { get; }

            internal int ProsperityValue { get; }

            internal bool IsCountryToHometown { get; }

            internal int Order { get; }
        }
    }
}
