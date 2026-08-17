using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SharedWarehouse.Core
{
    internal static class InventoryMerger
    {
        public static List<TEntry> MergeUnique<TEntry, TKey>(
            IEnumerable<List<TEntry>> inventories,
            Func<TEntry, TKey> keySelector,
            Func<TEntry, TEntry> cloneEntry,
            Action<TEntry, TEntry> mergeEntry)
        {
            if (inventories == null)
            {
                throw new ArgumentNullException(nameof(inventories));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (cloneEntry == null)
            {
                throw new ArgumentNullException(nameof(cloneEntry));
            }

            if (mergeEntry == null)
            {
                throw new ArgumentNullException(nameof(mergeEntry));
            }

            var seenInventories = new HashSet<List<TEntry>>(ReferenceEqualityComparer<List<TEntry>>.Instance);
            var entriesByKey = new Dictionary<TKey, TEntry>();
            var result = new List<TEntry>();

            foreach (var inventory in inventories)
            {
                if (inventory == null || !seenInventories.Add(inventory))
                {
                    continue;
                }

                foreach (var source in inventory)
                {
                    var key = keySelector(source);
                    if (entriesByKey.TryGetValue(key, out var target))
                    {
                        mergeEntry(target, source);
                        continue;
                    }

                    var clone = cloneEntry(source);
                    entriesByKey.Add(key, clone);
                    result.Add(clone);
                }
            }

            return result;
        }
    }

    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer()
        {
        }

        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }

        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }
    }
}
