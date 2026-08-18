using TerrainEditor.Core;
using UnityEngine;
using Utility.UI;

namespace TerrainEditor.Runtime
{
    internal sealed class RatopiaTerrainEditorGateway : ITerrainEditorGateway
    {
        private static GameMgr Game => GameMgr.Instance;

        private static DebugMgr Debug => DebugMgr.Instance;

        private static TileMgr Tile => Game?._TileMgr;

        private static CameraMgr Camera => Game?._CamMgr;

        private static PallateMgr Palette => Debug?._PallateMgr;

        public bool IsReady
        {
            get
            {
                var palette = Palette;
                return Game != null
                    && Tile != null
                    && Camera != null
                    && Camera.m_MainCam != null
                    && palette != null
                    && palette.Obj_Main != null
                    && palette.m_Icons != null;
            }
        }

        public bool IsGameMenuOpen
        {
            get
            {
                var menu = GameMenuMgr.Instance;
                return menu != null && menu.IsActivate;
            }
        }

        public object SessionToken => Tile;

        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        public ITerrainEditorSession CaptureSession()
        {
            return new RatopiaTerrainEditorSession(Tile, Camera, Palette);
        }

        private sealed class RatopiaTerrainEditorSession : ITerrainEditorSession
        {
            private readonly TileMgr _tile;
            private readonly CameraMgr _camera;
            private readonly PallateMgr _palette;

            public RatopiaTerrainEditorSession(TileMgr tile, CameraMgr camera, PallateMgr palette)
            {
                _tile = tile;
                _camera = camera;
                _palette = palette;
            }

            public object Token => _tile;

            public bool SandboxMode
            {
                get => _tile.IsSandBoxMode;
                set
                {
                    if (_tile != null)
                    {
                        _tile.IsSandBoxMode = value;
                    }
                }
            }

            public float Zoom
            {
                get => _camera.m_MainCam.orthographicSize;
                set
                {
                    if (_camera != null && _camera.m_MainCam != null)
                    {
                        _camera.ZoomSizeUpdate(value);
                    }
                }
            }

            public bool PaletteVisible
            {
                get => _palette.Obj_Main.activeSelf;
                set
                {
                    if (_palette != null && _palette.Obj_Main != null)
                    {
                        _palette.Obj_Main.SetActive(value);
                    }
                }
            }

            public void ResetPaletteSelection()
            {
                if (_palette == null)
                {
                    return;
                }

                var icons = _palette.m_Icons;
                if (icons != null)
                {
                    foreach (var icon in icons)
                    {
                        if (icon != null && icon.m_Outline != null && icon.m_Outline.enabled)
                        {
                            icon.MouseUp();
                        }
                    }
                }

                _palette.m_BrushType = 0;
            }
        }
    }
}
