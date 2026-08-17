using System;
using System.Collections.Generic;
using UnityEngine;
using WireThroughWalls.Core;

namespace WireThroughWalls.Runtime
{
    internal sealed class TransparencyScope : IDisposable
    {
        private readonly RestorationStack _restorations;
        private bool _disposed;

        private TransparencyScope(
            RestorationStack restorations,
            bool duplicateCompletedWire)
        {
            _restorations = restorations;
            DuplicateCompletedWire = duplicateCompletedWire;
        }

        internal bool DuplicateCompletedWire { get; }

        internal static TransparencyScope Create(
            WireOverlayCoordinator coordinator,
            BuildInfo candidate,
            IEnumerable<Vector2Int> positions)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            var targets = new HashSet<Vector2Int>(positions ?? Array.Empty<Vector2Int>());
            var candidateIsWire = WireOverlayCoordinator.IsWire(candidate);
            var restorations = new RestorationStack();
            try
            {
                restorations.Push(WireActionScope.EnterTransparencyView());
                var snapshot = NodeStateSnapshot.Capture(coordinator.TileManager, targets);
                restorations.Push(snapshot);
                restorations.Push(ScopedListMask<BP_Building>.RemoveWhere(
                    coordinator.BuildingManager.List_BP_BlueBuilding,
                    blueprint =>
                        blueprint != null &&
                        OverlayRules.CanBlueprintsShare(
                            candidateIsWire,
                            WireOverlayCoordinator.IsWire(blueprint.m_Info)) &&
                        WireOverlayCoordinator.Overlaps(blueprint.List_BuildPos, targets)));

                if (candidateIsWire)
                {
                    snapshot.HideOccupancy(
                        coordinator.HasOverlayableOccupancyAt,
                        position => coordinator.ShouldHideBuildTypeAt(candidate, position));
                }
                else
                {
                    snapshot.HideOccupancy(
                        coordinator.ShouldHideWireOccupancyForForegroundAt,
                        null);
                }

                return new TransparencyScope(
                    restorations,
                    candidateIsWire && coordinator.HasCompletedWireAt(targets));
            }
            catch (Exception creationError)
            {
                try
                {
                    restorations.Dispose();
                }
                catch (Exception restorationError)
                {
                    throw new AggregateException(
                        "Creating the transparency view failed and restoration also reported an error.",
                        creationError,
                        restorationError);
                }

                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _restorations.Dispose();
        }
    }

    internal sealed class LifecyclePatchState : IDisposable
    {
        private readonly HashSet<Vector2Int> _positions;
        private readonly RestorationStack _scopes = new RestorationStack();
        private bool _disposed;

        internal LifecyclePatchState(
            WireOverlayCoordinator coordinator,
            IEnumerable<Vector2Int> positions)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            _positions = new HashSet<Vector2Int>(positions ?? Array.Empty<Vector2Int>());
        }

        internal HashSet<Vector2Int> Positions => _positions;

        internal void AddPositions(IEnumerable<Vector2Int> positions)
        {
            if (positions != null)
            {
                _positions.UnionWith(positions);
            }
        }

        internal void AddScope(IDisposable scope)
        {
            if (scope != null)
            {
                _scopes.Push(scope);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _scopes.Dispose();
        }
    }
}
