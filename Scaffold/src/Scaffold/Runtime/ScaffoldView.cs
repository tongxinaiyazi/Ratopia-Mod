using UnityEngine;

namespace ScaffoldMod.Runtime
{
    internal sealed class ScaffoldView : MonoBehaviour
    {
        internal int X { get; private set; }

        internal int Y { get; private set; }

        internal C_Tile SelectionProxy { get; private set; }

        internal static ScaffoldView Create(int x, int y)
        {
            var gameObject = new GameObject($"ScaffoldOverlay_{x}_{y}");
            gameObject.transform.position = new Vector3(x, y, -2.55f);
            gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            var tileLayer = LayerMask.NameToLayer("Tile");
            if (tileLayer >= 0)
            {
                gameObject.layer = tileLayer;
            }

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ScaffoldAssets.World;
            renderer.sortingOrder = 8;

            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.7f, 0.78f);

            var proxy = gameObject.AddComponent<C_Tile>();
            proxy.m_Box = collider;
            proxy.Spr_Main = renderer;
            proxy.Tf = gameObject.transform;
            proxy.Obj = gameObject;
            proxy.m_X = x;
            proxy.m_Y = y;
            proxy.m_Pos = new Vector2(x, y);
            proxy.m_TileType = TileType.Ladder;
            proxy.m_Info = GameMgr.Instance?._DB_Mgr?.GetTileInfo(TileType.Ladder);
            proxy.IsNatureLadder = true;

            var view = gameObject.AddComponent<ScaffoldView>();
            view.X = x;
            view.Y = y;
            view.SelectionProxy = proxy;
            return view;
        }
    }
}
