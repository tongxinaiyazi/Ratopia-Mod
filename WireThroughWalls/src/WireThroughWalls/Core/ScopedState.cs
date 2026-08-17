using System;
using System.Collections.Generic;

namespace WireThroughWalls.Core
{
    internal sealed class ScopedFlag
    {
        private int _depth;

        internal bool IsActive => _depth > 0;

        internal IDisposable Enter()
        {
            _depth++;
            return new CallbackScope(() => _depth--);
        }
    }

    internal sealed class ScopedMembership<T>
    {
        private readonly Dictionary<T, int> _counts = new Dictionary<T, int>();

        internal bool Contains(T item)
        {
            return _counts.ContainsKey(item);
        }

        internal IDisposable Enter(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var entered = new HashSet<T>(items);
            foreach (var item in entered)
            {
                _counts.TryGetValue(item, out var count);
                _counts[item] = count + 1;
            }

            return new CallbackScope(() =>
            {
                foreach (var item in entered)
                {
                    var count = _counts[item] - 1;
                    if (count == 0)
                    {
                        _counts.Remove(item);
                    }
                    else
                    {
                        _counts[item] = count;
                    }
                }
            });
        }
    }

    internal sealed class CallbackScope : IDisposable
    {
        private Action _callback;

        internal CallbackScope(Action callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public void Dispose()
        {
            var callback = _callback;
            if (callback == null)
            {
                return;
            }

            _callback = null;
            callback();
        }
    }
}
