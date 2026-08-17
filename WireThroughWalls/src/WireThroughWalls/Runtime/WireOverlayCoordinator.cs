using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WireThroughWalls.Core;

namespace WireThroughWalls.Runtime
{
    internal sealed class WireOverlayCoordinator
    {
        internal BuildingMgr BuildingManager { get; private set; }

        internal TileMgr TileManager { get; private set; }

        internal bool IsReady => BuildingManager != null && TileManager != null;

        internal void Initialize(BuildingMgr buildingManager, TileMgr tileManager)
        {
            BuildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            TileManager = tileManager ?? throw new ArgumentNullException(nameof(tileManager));
        }

        internal void Reset()
        {
            BuildingManager = null;
            TileManager = null;
        }

        internal static bool IsWire(BuildInfo info)
        {
            return info != null &&
                   (info.Name == BuildingName.HeavyWire || info.Ability == BuildAbility.HeavyWire);
        }

        internal List<Vector2Int> GetPlacementPositions(BuildInfo info, Transform transform)
        {
            if (!IsReady || info == null || transform == null)
            {
                return new List<Vector2Int>();
            }

            var world = transform.position;
            var origin = new Vector2Int(Mathf.CeilToInt(world.x), Mathf.CeilToInt(world.y));
            return BuildingManager.GetTileList(true, origin, info.GetSize());
        }

        internal List<Vector2Int> GetBuildPositions(BuildInfo info, Vector2 position)
        {
            if (!IsReady || info == null)
            {
                return new List<Vector2Int>();
            }

            var origin = new Vector2Int(
                Mathf.CeilToInt(position.x),
                Mathf.CeilToInt(position.y - 0.5f));
            return BuildingManager.GetTileList(false, origin, info.GetSize());
        }

        internal bool HasCompletedWireAt(IEnumerable<Vector2Int> positions)
        {
            if (!IsReady || positions == null || BuildingManager.List_HeavyWire == null)
            {
                return false;
            }

            var targets = new HashSet<Vector2Int>(positions);
            return BuildingManager.List_HeavyWire.Any(wire =>
                wire != null && Overlaps(wire.List_BuildPos, targets));
        }

        internal bool HasWirePresenceAt(IEnumerable<Vector2Int> positions)
        {
            if (!IsReady || positions == null)
            {
                return false;
            }

            var targets = new HashSet<Vector2Int>(positions);
            if (HasCompletedWireAt(targets))
            {
                return true;
            }

            return BuildingManager.List_BP_BlueBuilding != null &&
                   BuildingManager.List_BP_BlueBuilding.Any(blueprint =>
                       blueprint != null &&
                       IsWire(blueprint.m_Info) &&
                       Overlaps(blueprint.List_BuildPos, targets));
        }

        internal bool RequiresCoordination(BuildInfo candidate, IEnumerable<Vector2Int> positions)
        {
            return OverlayRules.RequiresCoordination(IsWire(candidate), HasWirePresenceAt(positions));
        }

        internal bool HasForegroundOwnerAt(Vector2Int position)
        {
            if (!IsReady)
            {
                return false;
            }

            if (BuildingManager.List_Building != null &&
                BuildingManager.List_Building.Any(building =>
                                  building != null &&
                                  !IsWire(building.m_Info) &&
                                  Contains(building.List_BuildPos, position)))
            {
                return true;
            }

            return BuildingManager.List_BP_BlueBuilding != null &&
                   BuildingManager.List_BP_BlueBuilding.Any(blueprint =>
                                          blueprint != null &&
                                          blueprint.m_Info != null &&
                                          !IsWire(blueprint.m_Info) &&
                                          blueprint.m_Box != null &&
                                          blueprint.m_Box.enabled &&
                                          Contains(blueprint.List_BuildPos, position));
        }

        internal bool HasOverlayableOccupancyAt(Vector2Int position)
        {
            if (!IsReady)
            {
                return false;
            }

            if (HasForegroundOwnerAt(position) ||
                HasWirePresenceAt(new[] { position }))
            {
                return true;
            }

            var tile = TileManager.GetTile(position);
            return tile != null && IsRoadTile(tile.m_TileType);
        }

        internal bool ShouldHideBuildTypeAt(BuildInfo candidate, Vector2Int position)
        {
            return OverlayRules.ShouldHideBuildType(IsWire(candidate), HasForegroundOwnerAt(position));
        }

        internal bool ShouldHideWireOccupancyForForegroundAt(Vector2Int position)
        {
            return !HasForegroundOwnerAt(position) && HasWirePresenceAt(new[] { position });
        }

        private static bool IsRoadTile(TileType tileType)
        {
            var value = (int)tileType;
            return value >= (int)TileType.Woodroad && value <= (int)TileType.Goldroad;
        }

        internal int ReevaluateBlueprints()
        {
            if (!IsReady || BuildingManager.List_BP_BlueBuilding == null)
            {
                return 0;
            }

            var reevaluated = 0;
            var blueprints = BuildingManager.List_BP_BlueBuilding.ToArray();
            foreach (var blueprint in blueprints)
            {
                if (blueprint == null ||
                    blueprint.m_Info == null ||
                    !RequiresCoordination(blueprint.m_Info, blueprint.List_BuildPos))
                {
                    continue;
                }

                try
                {
                    blueprint.MakeEnableList();
                    reevaluated++;
                }
                catch (Exception error)
                {
                    Plugin.LogPatchError($"读档后重评蓝图 ID={blueprint.m_ID}", error);
                }
            }

            return reevaluated;
        }

        internal static bool Overlaps(IEnumerable<Vector2Int> positions, HashSet<Vector2Int> targets)
        {
            return positions != null && targets != null && positions.Any(targets.Contains);
        }

        private static bool Contains(IEnumerable<Vector2Int> positions, Vector2Int target)
        {
            return positions != null && positions.Contains(target);
        }

    }
}
