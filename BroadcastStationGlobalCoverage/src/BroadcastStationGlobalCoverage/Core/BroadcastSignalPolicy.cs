using System;
using System.Collections.Generic;

namespace BroadcastStationGlobalCoverage.Core
{
    internal static class BroadcastSignalPolicy
    {
        internal const int BroadcastStationId = 309;

        internal static bool IsBroadcastStation(int buildingName)
        {
            return buildingName == BroadcastStationId;
        }

        internal static void AppendMissing<T>(
            IList<T> target,
            IEnumerable<T> source,
            Func<T, bool> predicate)
            where T : class
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (var item in source)
            {
                if (item != null && predicate(item) && !target.Contains(item))
                {
                    target.Add(item);
                }
            }
        }

        internal static T FindNearest<T>(
            IEnumerable<T> candidates,
            Func<T, bool> predicate,
            Func<T, float> squaredDistance,
            T fallback)
            where T : class
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (squaredDistance == null)
            {
                throw new ArgumentNullException(nameof(squaredDistance));
            }

            var selected = fallback;
            var selectedDistance = float.PositiveInfinity;
            foreach (var candidate in candidates)
            {
                if (candidate == null || !predicate(candidate))
                {
                    continue;
                }

                var distance = squaredDistance(candidate);
                if (distance < selectedDistance)
                {
                    selected = candidate;
                    selectedDistance = distance;
                }
            }

            return selected;
        }
    }
}
