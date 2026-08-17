using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WireThroughWalls.Core;

namespace WireThroughWalls.Runtime
{
    internal sealed class PortOverlayRegistry
    {
        private readonly PortOccupancyIndex<Vector2Int, ElecPort> _index =
            new PortOccupancyIndex<Vector2Int, ElecPort>();
        private readonly HashSet<Vector2Int> _retryPositions = new HashSet<Vector2Int>();
        private BuildingMgr _manager;
        private TileMgr _tileManager;
        private int _reconciliationDepth;

        internal bool IsReady => _manager != null && _tileManager != null;

        internal void Initialize(BuildingMgr manager, TileMgr tileManager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (tileManager == null)
            {
                throw new ArgumentNullException(nameof(tileManager));
            }

            if (_manager != null && !ReferenceEquals(_manager, manager))
            {
                Reset();
            }

            _manager = manager;
            _tileManager = tileManager;
        }

        internal void Reset()
        {
            _index.Clear();
            _retryPositions.Clear();
            _manager = null;
            _tileManager = null;
            _reconciliationDepth = 0;
        }

        internal void Register(ElecPort port, string stage)
        {
            if (!IsReady || port == null)
            {
                return;
            }

            var position = port.GetPos();
            _index.Register(position, port.m_ID, ClassifyOwner(port, position), port);
            if (_reconciliationDepth == 0 && _index.GetOwners(position).Count > 1)
            {
                TryReconcile(position, stage);
            }
        }

        internal PortRemovalState CaptureRemoval(ElecPort port)
        {
            return port == null
                ? null
                : new PortRemovalState(this, new[] { new PortRemoval(port.GetPos(), port.m_ID) });
        }

        internal PortRemovalState CaptureRemoval(IEnumerable<ElecPort> ports)
        {
            if (ports == null)
            {
                return null;
            }

            var removals = ports
                .Where(port => port != null)
                .Select(port => new PortRemoval(port.GetPos(), port.m_ID))
                .Distinct(PortRemovalComparer.Instance)
                .ToArray();
            return removals.Length == 0 ? null : new PortRemovalState(this, removals);
        }

        internal int ValidateAllRegistered(string stage)
        {
            return ValidatePositions(_index.Positions.Concat(_retryPositions).Distinct().ToArray(), stage);
        }

        internal int ValidateOverlaps(string stage)
        {
            return ValidatePositions(
                _index.MultiOwnerPositions.Concat(_retryPositions).Distinct().ToArray(),
                stage);
        }

        internal void CompleteRemoval(IEnumerable<PortRemoval> removals, string stage)
        {
            if (!IsReady || removals == null)
            {
                return;
            }

            foreach (var group in removals.GroupBy(removal => removal.Position))
            {
                foreach (var removal in group)
                {
                    _index.Remove(removal.Position, removal.OwnerId);
                }

                TryReconcile(group.Key, stage);
            }
        }

        private int ValidatePositions(IEnumerable<Vector2Int> positions, string stage)
        {
            if (!IsReady || positions == null)
            {
                return 0;
            }

            var validated = 0;
            foreach (var position in positions)
            {
                if (TryReconcile(position, stage))
                {
                    validated++;
                }
            }

            return validated;
        }

        private bool TryReconcile(Vector2Int position, string stage)
        {
            try
            {
                _reconciliationDepth++;
                Reconcile(position);
                _retryPositions.Remove(position);
                return true;
            }
            catch (Exception error)
            {
                _retryPositions.Add(position);
                var owners = string.Join(", ", _index.GetOwners(position)
                    .Select(owner => $"{owner.Kind}#{owner.OwnerId}"));
                Plugin.LogPortCoordinationError(position, owners, stage, error);
                return false;
            }
            finally
            {
                _reconciliationDepth--;
            }
        }

        private void Reconcile(Vector2Int position)
        {
            var owners = _index.GetOwners(position)
                .Where(owner => owner.Value != null)
                .ToArray();
            if (owners.Length == 0)
            {
                return;
            }

            var representative = owners[0];
            var lines = new List<ElecLine_Info>();
            ElecLine_Info primary = null;
            var topologyChanged = false;

            foreach (var owner in owners)
            {
                var line = _manager.SearchElecInfo(owner.OwnerId);
                if (line == null)
                {
                    _manager.NewConnectCheck(owner.Value, 0f);
                    line = _manager.SearchElecInfo(owner.OwnerId);
                    topologyChanged = line != null;
                }

                if (line == null)
                {
                    throw new InvalidOperationException($"端口 {owner.OwnerId} 未能恢复到电网线路。");
                }

                if (owner.OwnerId == representative.OwnerId)
                {
                    primary = line;
                }

                if (!lines.Contains(line))
                {
                    lines.Add(line);
                }
            }

            primary = primary ?? lines[0];
            foreach (var secondary in lines.Where(line => !ReferenceEquals(line, primary)).ToArray())
            {
                _manager.MergeTwoElecLine(primary, secondary);
                topologyChanged = true;
            }

            var representativeChanged =
                !_manager.Dic_PortTileMap.TryGetValue(position, out var current) ||
                current == null ||
                current.m_ID != representative.OwnerId ||
                current.m_PortType != representative.Value.m_PortType ||
                current.m_X != position.x ||
                current.m_Y != position.y;

            if (representativeChanged)
            {
                _manager.Dic_PortTileMap[position] = new ElecPort(
                    representative.Value.m_PortType,
                    position,
                    representative.OwnerId);
            }

            if (topologyChanged || representativeChanged)
            {
                _manager.RefreshWire(position);
            }

            if (topologyChanged)
            {
                primary.ActRefreshByDynamo();
            }
        }

        private PortOwnerKind ClassifyOwner(ElecPort port, Vector2Int position)
        {
            if (_manager.List_HeavyWire != null && _manager.List_HeavyWire.Any(wire =>
                    wire != null &&
                    wire.m_ID == port.m_ID &&
                    wire.List_BuildPos != null &&
                    wire.List_BuildPos.Contains(position)))
            {
                return PortOwnerKind.HeavyWire;
            }

            var tile = _tileManager.GetTile(position);
            if (tile != null && tile.m_TileType == TileType.Wireroad && tile.GetID() == port.m_ID)
            {
                return PortOwnerKind.WireRoad;
            }

            return PortOwnerKind.ForegroundBuilding;
        }
    }

    internal readonly struct PortRemoval
    {
        internal PortRemoval(Vector2Int position, int ownerId)
        {
            Position = position;
            OwnerId = ownerId;
        }

        internal Vector2Int Position { get; }

        internal int OwnerId { get; }
    }

    internal sealed class PortRemovalState : IDisposable
    {
        private PortOverlayRegistry _registry;
        private readonly PortRemoval[] _removals;
        private string _stage;

        internal PortRemovalState(PortOverlayRegistry registry, PortRemoval[] removals)
        {
            _registry = registry;
            _removals = removals;
        }

        internal void SetStage(string stage)
        {
            _stage = stage;
        }

        internal void Cancel()
        {
            _registry = null;
        }

        public void Dispose()
        {
            var registry = _registry;
            _registry = null;
            registry?.CompleteRemoval(_removals, _stage ?? "DeleteConnectCheck");
        }
    }

    internal sealed class PortRemovalComparer : IEqualityComparer<PortRemoval>
    {
        internal static readonly PortRemovalComparer Instance = new PortRemovalComparer();

        public bool Equals(PortRemoval left, PortRemoval right)
        {
            return left.OwnerId == right.OwnerId && left.Position == right.Position;
        }

        public int GetHashCode(PortRemoval value)
        {
            unchecked
            {
                return (value.Position.GetHashCode() * 397) ^ value.OwnerId;
            }
        }
    }
}
