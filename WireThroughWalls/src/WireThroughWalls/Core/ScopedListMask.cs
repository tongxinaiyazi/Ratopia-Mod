using System;
using System.Collections.Generic;

namespace WireThroughWalls.Core
{
    internal sealed class ScopedListMask<T> : IDisposable
    {
        private readonly IList<T> _list;
        private readonly List<RemovedEntry> _removed;
        private bool _disposed;

        private ScopedListMask(IList<T> list, List<RemovedEntry> removed)
        {
            _list = list;
            _removed = removed;
        }

        internal static ScopedListMask<T> RemoveWhere(IList<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var removed = new List<RemovedEntry>();
            for (var index = list.Count - 1; index >= 0; index--)
            {
                if (predicate(list[index]))
                {
                    removed.Insert(0, new RemovedEntry(index, list[index]));
                    list.RemoveAt(index);
                }
            }

            return new ScopedListMask<T>(list, removed);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var entry in _removed)
            {
                _list.Insert(Math.Min(entry.Index, _list.Count), entry.Item);
            }
        }

        private readonly struct RemovedEntry
        {
            internal RemovedEntry(int index, T item)
            {
                Index = index;
                Item = item;
            }

            internal int Index { get; }

            internal T Item { get; }
        }
    }
}
