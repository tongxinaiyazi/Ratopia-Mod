using System.Collections.Generic;
using System.Linq;

namespace WireThroughWalls.Core
{
    internal enum PortOwnerKind
    {
        ForegroundBuilding = 0,
        WireRoad = 1,
        HeavyWire = 2
    }

    internal readonly struct PortOwnerRecord<TValue>
    {
        internal PortOwnerRecord(int ownerId, PortOwnerKind kind, TValue value)
        {
            OwnerId = ownerId;
            Kind = kind;
            Value = value;
        }

        internal int OwnerId { get; }

        internal PortOwnerKind Kind { get; }

        internal TValue Value { get; }
    }

    internal sealed class PortOccupancyIndex<TPosition, TValue>
    {
        private readonly Dictionary<TPosition, List<PortOwnerRecord<TValue>>> _owners =
            new Dictionary<TPosition, List<PortOwnerRecord<TValue>>>();

        internal IEnumerable<TPosition> MultiOwnerPositions =>
            _owners.Where(pair => pair.Value.Count > 1).Select(pair => pair.Key);

        internal IEnumerable<TPosition> Positions => _owners.Keys;

        internal void Register(TPosition position, int ownerId, PortOwnerKind kind, TValue value)
        {
            if (!_owners.TryGetValue(position, out var entries))
            {
                entries = new List<PortOwnerRecord<TValue>>();
                _owners.Add(position, entries);
            }

            var existing = entries.FindIndex(entry => entry.OwnerId == ownerId);
            var record = new PortOwnerRecord<TValue>(ownerId, kind, value);
            if (existing >= 0)
            {
                entries[existing] = record;
            }
            else
            {
                entries.Add(record);
            }
        }

        internal bool Remove(TPosition position, int ownerId)
        {
            if (!_owners.TryGetValue(position, out var entries))
            {
                return false;
            }

            var removed = entries.RemoveAll(entry => entry.OwnerId == ownerId) > 0;
            if (entries.Count == 0)
            {
                _owners.Remove(position);
            }

            return removed;
        }

        internal IReadOnlyList<PortOwnerRecord<TValue>> GetOwners(TPosition position)
        {
            if (!_owners.TryGetValue(position, out var entries))
            {
                return new PortOwnerRecord<TValue>[0];
            }

            return entries
                .OrderBy(entry => (int)entry.Kind)
                .ThenBy(entry => entry.OwnerId)
                .ToArray();
        }

        internal bool TryGetRepresentative(TPosition position, out PortOwnerRecord<TValue> representative)
        {
            var owners = GetOwners(position);
            if (owners.Count == 0)
            {
                representative = default;
                return false;
            }

            representative = owners[0];
            return true;
        }

        internal void Clear()
        {
            _owners.Clear();
        }
    }
}
