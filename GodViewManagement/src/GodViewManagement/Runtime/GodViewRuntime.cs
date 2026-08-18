using BepInEx.Configuration;
using BepInEx.Logging;
using CasselGames.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GodViewManagement.Runtime
{
    internal sealed class GodViewRuntime
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigEntry<Key> _toggleKey;
        private readonly ConfigFile _config;
        private readonly ManagementModeState _mode = new ManagementModeState();
        private readonly InputBindingScanner _bindingScanner = new InputBindingScanner();
        private readonly HudVisibilityState _hudVisibility = new HudVisibilityState();
        private readonly GodViewCameraController _camera = new GodViewCameraController();
        private readonly RemotePanelSession _remotePanel = new RemotePanelSession();

        private TileMgr _tileManager;
        private BuildingMgr _buildingManager;
        private CameraMgr _cameraManager;
        private T_Queen _queen;
        private BuildMidUI _buildMidUi;
        private GodViewHud _hud;
        private string _bindingConflict;
        private bool _sessionReady;

        internal bool IsModeEnabled => _mode.IsEnabled;

        public GodViewRuntime(ManualLogSource logger, ConfigEntry<Key> toggleKey, ConfigFile config)
        {
            _logger = logger;
            _toggleKey = toggleKey;
            _config = config;
        }

        public void Tick(TileMgr patchedTileManager)
        {
            var game = GameMgr.Instance;
            if (game == null
                || patchedTileManager == null
                || game._TileMgr == null
                || !ReferenceEquals(game._TileMgr, patchedTileManager)
                || game._BuildingMgr == null
                || game._CamMgr == null
                || game._T_UnitMgr?.m_Queen == null
                || game._BuildMidUI == null)
            {
                ResetSession();
                return;
            }

            if (patchedTileManager.m_GameLoading)
            {
                ResetSession();
                return;
            }

            if (!_sessionReady || !ReferenceEquals(_tileManager, patchedTileManager))
            {
                InitializeSession(game, patchedTileManager);
            }

            if (_hud != null && !_hud.IsAlive)
            {
                _hud = null;
            }

            var visibilityInputHandled = HandleHudVisibilityHotkey();

            if (!_hudVisibility.IsHidden && _hud == null)
            {
                _hud = GodViewHud.TryCreate(game, OpenSettings, RestoreDefaultBinding, HideHud, CloseSettings);
                _hud?.Refresh(_mode.IsEnabled, _toggleKey.Value, _bindingConflict);
            }

            _remotePanel.Tick(_buildMidUi);

            if (_hud != null && _hud.IsCapturing)
            {
                ProcessBindingCapture();
                return;
            }

            if (!visibilityInputHandled)
            {
                HandleToggleHotkey();
            }
            if (!_mode.IsEnabled)
            {
                return;
            }

            if (_hud == null || !_hud.SettingsOpen)
            {
                _camera.Tick();
                TryOpenRemoteBuilding();
            }
        }

        public bool ShouldBlockQueenAction(BuildMidUI panel)
        {
            return _remotePanel.ShouldBlockQueenAction(panel);
        }

        public void FailSafeReset()
        {
            _remotePanel.Clear();
            try
            {
                _camera.Disable();
            }
            finally
            {
                _mode.Disable();
                var hud = _hud;
                if (hud != null && hud.IsAlive)
                {
                    hud.EndCapture();
                    hud.HideSettings();
                    hud.Refresh(false, _toggleKey.Value, _bindingConflict);
                }
                else
                {
                    _hud = null;
                }
            }
        }

        public void Dispose()
        {
            ResetSession();
        }

        private void InitializeSession(GameMgr game, TileMgr tileManager)
        {
            ResetSession();
            _tileManager = tileManager;
            _buildingManager = game._BuildingMgr;
            _cameraManager = game._CamMgr;
            _queen = game._T_UnitMgr.m_Queen;
            _buildMidUi = game._BuildMidUI;
            _mode.ObserveSession(tileManager);
            _bindingConflict = _bindingScanner.FindConflict(_toggleKey.Value);
            _sessionReady = true;
            _logger.LogInfo("检测到已载入的游戏会话；上帝视角保持关闭。设置 HUD 已准备创建。");
        }

        private void ResetSession()
        {
            if (!_sessionReady && _hud == null && !_mode.IsEnabled && !_remotePanel.IsOpen)
            {
                return;
            }

            FailSafeReset();
            _hud?.Dispose();
            _hud = null;
            _tileManager = null;
            _buildingManager = null;
            _cameraManager = null;
            _queen = null;
            _buildMidUi = null;
            _bindingConflict = null;
            _sessionReady = false;
            _mode.Reset();
            _hudVisibility.Reset();
        }

        private void ToggleMode()
        {
            if (_mode.IsEnabled)
            {
                _remotePanel.Clear();
                _camera.Disable();
                _mode.Disable();
                _logger.LogInfo("上帝视角管理已关闭；已恢复原版输入与相机跟随。");
            }
            else
            {
                _camera.Enable(_cameraManager, _tileManager, _queen);
                _mode.Toggle();
                _logger.LogInfo("上帝视角管理已开启；WASD 与屏幕边缘现在只移动相机。");
            }

            _hud?.Refresh(_mode.IsEnabled, _toggleKey.Value, _bindingConflict);
        }

        private void HandleToggleHotkey()
        {
            if (!_sessionReady
                || !string.IsNullOrWhiteSpace(_bindingConflict)
                || (_hud?.SettingsOpen ?? false)
                || _remotePanel.IsOpen
                || IsOtherModalPanelOpen())
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && _toggleKey.Value != Key.None && keyboard[_toggleKey.Value].wasPressedThisFrame)
            {
                ToggleMode();
            }
        }

        private bool HandleHudVisibilityHotkey()
        {
            if (!_sessionReady || _toggleKey.Value == Key.None)
            {
                return false;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            var shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            var togglePressed = keyboard[_toggleKey.Value].wasPressedThisFrame;
            if (!_hudVisibility.TryToggle(shiftPressed, togglePressed))
            {
                return false;
            }

            if (_hudVisibility.IsHidden)
            {
                HideHudObjects();
                _logger.LogInfo($"HUD 已隐藏；按 Shift + {_toggleKey.Value} 可恢复显示。");
            }
            else
            {
                _logger.LogInfo("HUD 已恢复显示。");
            }

            return true;
        }

        private void OpenSettings()
        {
            if (!_sessionReady || _remotePanel.IsOpen || _hud == null || IsOtherModalPanelOpen())
            {
                return;
            }

            InputMgr.Instance?.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
            _hud.ShowSettings();
            _hud.Refresh(_mode.IsEnabled, _toggleKey.Value, _bindingConflict);
        }

        private void CloseSettings()
        {
            if (_hud == null)
            {
                return;
            }

            _hud.HideSettings();
            RestoreInputAfterSettings();
        }

        private void HideHud()
        {
            if (_hud == null)
            {
                return;
            }

            _hudVisibility.Hide();
            HideHudObjects();
            _logger.LogInfo($"HUD 已隐藏；按 Shift + {_toggleKey.Value} 可恢复显示。");
        }

        private void HideHudObjects()
        {
            var hud = _hud;
            if (hud != null)
            {
                hud.EndCapture();
                hud.HideSettings();
                hud.Dispose();
                _hud = null;
            }

            RestoreInputAfterSettings();
        }

        private void RestoreInputAfterSettings()
        {
            if (_mode.IsEnabled)
            {
                InputMgr.Instance?.SetActionMap(InputMgr.INPUT_ACTIONMAP_UI);
            }
            else
            {
                InputMgr.Instance?.SetDefaultActionMap();
            }
        }

        private void RestoreDefaultBinding()
        {
            ApplyBinding(Key.M);
            _hud?.SetMessage(string.IsNullOrWhiteSpace(_bindingConflict)
                ? "已恢复默认按键 M。"
                : $"已恢复 M，但检测到冲突：{_bindingConflict}。");
        }

        private void ProcessBindingCapture()
        {
            if (!_bindingScanner.TryGetPressedKey(out var candidate))
            {
                return;
            }

            var conflict = _bindingScanner.FindConflict(candidate);
            var decision = InputBindingRules.Evaluate(candidate.ToString(), !string.IsNullOrWhiteSpace(conflict));
            if (decision == BindingDecision.Cancelled)
            {
                _hud.EndCapture();
                _hud.Refresh(_mode.IsEnabled, _toggleKey.Value, _bindingConflict);
                _hud.SetMessage("已取消重新绑定。");
                return;
            }

            if (decision == BindingDecision.ModifierOnly)
            {
                _hud.SetMessage("修饰键不能单独绑定，请按其他按键；Esc 取消。");
                return;
            }

            if (decision == BindingDecision.Conflict)
            {
                _hud.SetMessage($"该键与原版动作 {conflict} 冲突，请选择其他按键。");
                return;
            }

            ApplyBinding(candidate);
            _hud.EndCapture();
            _hud.Refresh(_mode.IsEnabled, _toggleKey.Value, _bindingConflict);
            _hud.SetMessage($"切换键已改为 {candidate}。配置已保存。");
        }

        private void ApplyBinding(Key key)
        {
            _toggleKey.Value = key;
            _config.Save();
            _bindingConflict = _bindingScanner.FindConflict(key);
            _hud?.Refresh(_mode.IsEnabled, key, _bindingConflict);
        }

        private void TryOpenRemoteBuilding()
        {
            var mouse = Mouse.current;
            var pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            var context = new RemoteClickContext(
                _mode.IsEnabled,
                _sessionReady,
                _tileManager == null || _tileManager.m_GameLoading,
                _hud?.SettingsOpen ?? false,
                _remotePanel.IsOpen,
                IsOtherModalPanelOpen(),
                pointerOverUi,
                _queen != null && _queen.IsQueenSafeState(),
                mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (!RemoteManagementRules.CanHandleClick(context))
            {
                return;
            }

            var position = _cameraManager.GetMouseIntPos();
            var building = _buildingManager.GetBuildingByBuildPos_Area(position);
            if (building == null || building.m_Info == null)
            {
                return;
            }

            var candidate = new BuildingCandidate(
                building.m_BuildState == BuildState.Basic,
                building.m_BuildInfoUI != null,
                building.m_Info.Ability == BuildAbility.Wallpaper,
                building.m_Info.Name == BuildingName.EnemyNexus);
            if (!RemoteManagementRules.IsEligibleBuilding(candidate))
            {
                return;
            }

            _queen.CharacterStop();
            _remotePanel.Open(_buildMidUi, building, keepGodViewInput: true);
            _logger.LogDebug($"已远程打开建筑配置：{building.m_Info.Name}，格子 {position}。");
        }

        private bool IsOtherModalPanelOpen()
        {
            return Time.timeScale <= 0f
                || (_buildMidUi != null && _buildMidUi.Obj_Main != null && _buildMidUi.Obj_Main.activeInHierarchy);
        }
    }
}
