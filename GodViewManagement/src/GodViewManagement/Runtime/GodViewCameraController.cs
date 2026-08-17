using CasselGames.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GodViewManagement.Runtime
{
    internal sealed class GodViewCameraController
    {
        private const float MoveSpeed = 12f;
        private const float EdgeThreshold = 24f;

        private CameraMgr _cameraManager;
        private TileMgr _tileManager;
        private T_Queen _queen;
        private bool _wasFollowingQueen;
        private Vector2 _savedPosition;

        public bool IsActive { get; private set; }

        public void Enable(CameraMgr cameraManager, TileMgr tileManager, T_Queen queen)
        {
            if (IsActive)
            {
                return;
            }

            _cameraManager = cameraManager;
            _tileManager = tileManager;
            _queen = queen;
            _wasFollowingQueen = cameraManager.m_FixQueen;
            _savedPosition = cameraManager.Tf_Camera.position;
            queen.CharacterStop();
            cameraManager.m_FixQueen = false;
            InputMgr.Instance?.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
            IsActive = true;
        }

        public void Tick()
        {
            if (!IsActive || _cameraManager == null || _tileManager == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var x = 0f;
            var y = 0f;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed) x -= 1f;
                if (keyboard.dKey.isPressed) x += 1f;
                if (keyboard.sKey.isPressed) y -= 1f;
                if (keyboard.wKey.isPressed) y += 1f;
            }

            if (mouse != null)
            {
                var position = mouse.position.ReadValue();
                if (position.x <= EdgeThreshold) x -= 1f;
                if (position.x >= Screen.width - EdgeThreshold) x += 1f;
                if (position.y <= EdgeThreshold) y -= 1f;
                if (position.y >= Screen.height - EdgeThreshold) y += 1f;
            }

            var delta = CameraPanMath.CalculateDelta(x, y, MoveSpeed, Time.unscaledDeltaTime);
            if (delta.X == 0f && delta.Y == 0f)
            {
                return;
            }

            var current = (Vector2)_cameraManager.Tf_Camera.position;
            var requested = current + new Vector2(delta.X, delta.Y);
            var clamped = _cameraManager.Tf_Update_ByCut(requested, false);
            _tileManager.TileChunkEnable_Update(clamped);
        }

        public void Disable()
        {
            if (!IsActive)
            {
                return;
            }

            try
            {
                if (_cameraManager != null)
                {
                    _cameraManager.m_FixQueen = _wasFollowingQueen;
                    var restorePosition = _savedPosition;
                    if (_wasFollowingQueen && _queen != null && _queen.Tf != null)
                    {
                        var queenPosition = _queen.Tf.position;
                        restorePosition = new Vector2(queenPosition.x, queenPosition.y + _queen.m_CamBottomHeight);
                    }

                    var clamped = _cameraManager.Tf_Update_ByCut(restorePosition, false);
                    _tileManager?.TileChunkEnable_Update(clamped);
                }
            }
            finally
            {
                InputMgr.Instance?.SetDefaultActionMap();
                IsActive = false;
                _cameraManager = null;
                _tileManager = null;
                _queen = null;
            }
        }
    }
}
