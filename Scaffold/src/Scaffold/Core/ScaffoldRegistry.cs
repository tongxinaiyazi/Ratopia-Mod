using System.Collections.Generic;
using System.Linq;

namespace ScaffoldMod.Core
{
    internal sealed class ScaffoldRegistry
    {
        private readonly Dictionary<long, ScaffoldRecord> records = new Dictionary<long, ScaffoldRecord>();

        internal bool TryAdd(ScaffoldRecord record)
        {
            var key = CoordinateKey(record.X, record.Y);
            if (records.ContainsKey(key))
            {
                return false;
            }

            records.Add(key, record);
            return true;
        }

        internal bool TryGet(int x, int y, out ScaffoldRecord record)
        {
            return records.TryGetValue(CoordinateKey(x, y), out record);
        }

        internal bool TryRemove(int x, int y, out ScaffoldRecord record)
        {
            var key = CoordinateKey(x, y);
            if (!records.TryGetValue(key, out record))
            {
                return false;
            }

            records.Remove(key);
            return true;
        }

        internal void ReplaceWith(IEnumerable<ScaffoldRecord> replacement)
        {
            records.Clear();
            foreach (var record in replacement)
            {
                records[CoordinateKey(record.X, record.Y)] = record;
            }
        }

        internal IReadOnlyList<ScaffoldRecord> Snapshot()
        {
            return records.Values
                .OrderBy(record => record.X)
                .ThenBy(record => record.Y)
                .ToArray();
        }

        internal void Clear()
        {
            records.Clear();
        }

        private static long CoordinateKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}
