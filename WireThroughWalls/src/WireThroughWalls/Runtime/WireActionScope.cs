using System;
using System.Collections.Generic;
using UnityEngine;
using WireThroughWalls.Core;

namespace WireThroughWalls.Runtime
{
    internal static class WireActionScope
    {
        private static readonly ScopedMembership<Vector2Int> ProtectedTiles =
            new ScopedMembership<Vector2Int>();

        private static readonly ScopedFlag DemolitionSelection = new ScopedFlag();
        private static readonly ScopedFlag TransparencyView = new ScopedFlag();

        internal static bool IsDemolitionSelectionActive => DemolitionSelection.IsActive;

        internal static bool IsTransparencyActive => TransparencyView.IsActive;

        internal static IDisposable ProtectTiles(IEnumerable<Vector2Int> positions)
        {
            return ProtectedTiles.Enter(positions ?? Array.Empty<Vector2Int>());
        }

        internal static bool IsTileProtected(int x, int y)
        {
            return ProtectedTiles.Contains(new Vector2Int(x, y));
        }

        internal static IDisposable EnterDemolitionSelection()
        {
            return DemolitionSelection.Enter();
        }

        internal static IDisposable EnterTransparencyView()
        {
            return TransparencyView.Enter();
        }
    }
}
