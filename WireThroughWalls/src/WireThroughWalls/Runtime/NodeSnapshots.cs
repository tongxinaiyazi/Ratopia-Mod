using System;
using System.Collections.Generic;
using UnityEngine;

namespace WireThroughWalls.Runtime
{
    internal sealed class NodeStateSnapshot : IDisposable
    {
        private readonly List<Entry> _entries;
        private bool _disposed;

        private NodeStateSnapshot(List<Entry> entries)
        {
            _entries = entries;
        }

        internal static NodeStateSnapshot Capture(TileMgr tileManager, IEnumerable<Vector2Int> positions)
        {
            var entries = new List<Entry>();
            if (tileManager == null || positions == null)
            {
                return new NodeStateSnapshot(entries);
            }

            foreach (var position in new HashSet<Vector2Int>(positions))
            {
                var node = tileManager.GetNode(position);
                if (node != null)
                {
                    entries.Add(new Entry(
                        node,
                        node.m_TileType,
                        node.m_NodeType,
                        node.m_BuildType,
                        node.m_RailSlope,
                        node.m_WorldObj));
                }
            }

            return new NodeStateSnapshot(entries);
        }

        internal void HideOccupancy(
            Func<Vector2Int, bool> shouldHideOccupancy,
            Func<Vector2Int, bool> shouldHideBuildType)
        {
            if (shouldHideOccupancy == null)
            {
                throw new ArgumentNullException(nameof(shouldHideOccupancy));
            }

            foreach (var entry in _entries)
            {
                var position = new Vector2Int(entry.Node.x, entry.Node.y);
                if (shouldHideOccupancy(position))
                {
                    entry.Node.m_TileType = TileType.None;
                    entry.Node.m_NodeType = NodeType.None;
                }

                if (shouldHideBuildType != null &&
                    shouldHideBuildType(position) &&
                    entry.Node.m_BuildType != BuildType.SpecialMapObj)
                {
                    entry.Node.m_BuildType = BuildType.None;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var entry in _entries)
            {
                entry.Node.m_TileType = entry.TileType;
                entry.Node.m_NodeType = entry.NodeType;
                entry.Node.m_BuildType = entry.BuildType;
                entry.Node.m_RailSlope = entry.RailSlope;
                entry.Node.m_WorldObj = entry.WorldObject;
            }
        }

        private readonly struct Entry
        {
            internal Entry(
                C_Node node,
                TileType tileType,
                NodeType nodeType,
                BuildType buildType,
                int railSlope,
                WorldObject worldObject)
            {
                Node = node;
                TileType = tileType;
                NodeType = nodeType;
                BuildType = buildType;
                RailSlope = railSlope;
                WorldObject = worldObject;
            }

            internal C_Node Node { get; }

            internal TileType TileType { get; }

            internal NodeType NodeType { get; }

            internal BuildType BuildType { get; }

            internal int RailSlope { get; }

            internal WorldObject WorldObject { get; }
        }
    }
}
