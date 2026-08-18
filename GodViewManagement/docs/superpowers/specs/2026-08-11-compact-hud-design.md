# Compact HUD Design

## Goal

Reduce obstruction of Ratopia's original upper-right UI while preserving access to God View settings and allowing players to remove the Mod HUD completely during a session.

## Considered approaches

1. **Recommended: one compact settings launcher plus recoverable session hiding.** Remove the always-visible mode toggle, move a single `设置` button 420 reference pixels left from the right edge, add `隐藏 HUD` inside the settings panel, and restore it with `Shift + current toggle key`. Restarting or switching saves also restores the launcher.
2. Keep a tiny icon after hiding. This is easy to recover but does not satisfy a true hide request and still occupies original UI space.
3. Persist a hidden setting in BepInEx config. This remembers preference but can permanently hide the only settings entry and force users to edit a config file manually.

Approach 1 is selected because the user requested a usable, low-detail implementation without repeated questions. It gives a genuinely empty view while retaining an in-game recovery path.

## UI behavior

- Remove the permanent `上帝视角：开/关` HUD button. God View remains toggled with the configured hotkey.
- Keep one `设置` launcher, size 126 x 42 reference pixels, anchored at the top-right with X offset `-420` and Y offset `-16`.
- The centered settings panel grows to 600 x 320 reference pixels.
- The panel contains mode status, current binding, binding message, `重新绑定`, `恢复默认`, `隐藏 HUD`, and `关闭`.
- `隐藏 HUD` closes and destroys the HUD canvas for the current loaded session. It does not change God View mode, camera state, input isolation, or the configured toggle key.
- `Shift + current toggle key` toggles HUD visibility. The chord is processed before normal mode switching, so it never toggles God View in the same frame.
- A new game session or save switch resets HUD visibility to shown. No new save or BepInEx config field is added.

## Architecture and safety

- `HudVisibilityState` owns the pure visible/hidden session state and recognizes the recovery chord.
- `GodViewRuntime` processes visibility before HUD creation and before the normal toggle hotkey.
- `GodViewHud` owns only scene objects and callbacks. Its factory no longer accepts a mode-toggle callback and instead accepts a hide callback.
- Existing modal-panel, remote-building, queen-input, fail-safe, and lifecycle rules remain unchanged.
- Release version becomes `0.1.3`; the distribution and installation are replaced only after Release tests and package validation succeed while Ratopia is closed.
