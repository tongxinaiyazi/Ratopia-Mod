using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ScaffoldMod.Core
{
    internal static class ScaffoldRecordCodec
    {
        private const string Prefix = "v1|";

        internal static string Encode(IEnumerable<ScaffoldRecord> records)
        {
            var ordered = records
                .OrderBy(record => record.X)
                .ThenBy(record => record.Y)
                .Select(record => string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3}",
                    record.X,
                    record.Y,
                    record.ExpiryMinute,
                    record.UnderlyingNodeType));

            return Prefix + string.Join(";", ordered);
        }

        internal static IReadOnlyList<ScaffoldRecord> Decode(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload) || !payload.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return Array.Empty<ScaffoldRecord>();
            }

            var byCoordinate = new Dictionary<long, ScaffoldRecord>();
            var entries = payload.Substring(Prefix.Length).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var fields = entry.Split(',');
                if (fields.Length != 4 ||
                    !TryParse(fields[0], out var x) ||
                    !TryParse(fields[1], out var y) ||
                    !TryParse(fields[2], out var expiry) ||
                    !TryParse(fields[3], out var nodeType) ||
                    expiry < 0 || nodeType < 0 || nodeType > 4)
                {
                    continue;
                }

                byCoordinate[CoordinateKey(x, y)] = new ScaffoldRecord(x, y, expiry, nodeType);
            }

            return byCoordinate.Values
                .OrderBy(record => record.X)
                .ThenBy(record => record.Y)
                .ToArray();
        }

        private static bool TryParse(string value, out int parsed)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }

        private static long CoordinateKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}
