# Compact HUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the obstructive two-button HUD with a left-shifted settings launcher and a recoverable session-only hide function.

**Architecture:** Add a pure `HudVisibilityState`, route visibility input through `GodViewRuntime`, and simplify `GodViewHud` to one launcher plus the expanded settings panel. Preserve all existing mode and remote-management behavior.

**Tech Stack:** C# / net472, BepInEx 5, Harmony 2.9, Unity UI/TMP/Input System, xUnit, Mono.Cecil, PowerShell.

## Global Constraints

- Game directory is `E:\steam\steamapps\common\Ratopia` and Ratopia must be closed during installation.
- Locked game assembly SHA-256 remains `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Plugin GUID remains `cn.ratopia.godviewmanagement`; release version becomes `0.1.3`.
- HUD hiding is session-only and adds no save or configuration field.
- `Shift + current toggle key` restores hidden HUD without toggling God View.
- Release payload contains only `GodViewManagement.dll` and `README.md`.

---

### Task 1: Visibility and layout contracts

**Files:**
- Create: `src/GodViewManagement/Core/HudVisibilityState.cs`
- Create: `tests/GodViewManagement.Tests/HudVisibilityStateTests.cs`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`

**Interfaces:**
- Produces: `bool HudVisibilityState.IsHidden`, `Hide()`, `Show()`, `Reset()`, and `TryToggle(bool shiftPressed, bool togglePressed)`.
- Produces: `GodViewHud.TryCreate` parameters named `openSettings`, `restoreDefault`, `hideHud`, and `closeSettings`, with no `toggleMode` parameter.

- [ ] **Step 1: Write tests for default visible state, hide/show/reset, modifier-gated toggling, and HUD factory callback names.**
- [ ] **Step 2: Run the focused tests and verify RED because `HudVisibilityState` and `hideHud` do not exist.**
- [ ] **Step 3: Implement the minimal pure state and change the HUD factory signature; verify the focused tests GREEN.**

### Task 2: Runtime and scene UI behavior

**Files:**
- Modify: `src/GodViewManagement/Runtime/GodViewRuntime.cs`
- Modify: `src/GodViewManagement/Runtime/GodViewHud.cs`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`

**Interfaces:**
- Consumes: `HudVisibilityState.TryToggle(bool shiftPressed, bool togglePressed)`.
- Produces: one launcher button at `(-420, -16)`, `HideHud()`, and a 600 x 320 settings panel with four action buttons.

- [ ] **Step 1: Add a contract requiring X offset `-420`, `HideHud`, and removal of the `Mode` launcher; verify RED.**
- [ ] **Step 2: Process the Shift chord before normal toggle input and suppress HUD creation while hidden.**
- [ ] **Step 3: Replace the two launchers with one settings button, add mode status and the hide action inside the panel, and make reset show the HUD again.**
- [ ] **Step 4: Run focused visibility/UI contracts and then the complete Release test suite.**

### Task 3: 0.1.3 release and installation

**Files:**
- Modify: `src/GodViewManagement/Plugin.cs`
- Modify: `src/GodViewManagement/GodViewManagement.csproj`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`
- Modify: `README.md`
- Modify: `docs/TESTING.md`
- Modify: `scripts/Package.ps1`
- Modify: `NexusMods/NEXUS_PAGE_BILINGUAL.md`
- Modify: `NexusMods/NEXUS_UPLOAD_FIELDS.md`

**Interfaces:**
- Produces: `dist/上帝视角管理-v0.1.3-BepInEx5.zip` and installed plugin version `0.1.3`.

- [ ] **Step 1: Update version strings, controls, recovery instructions, changelog, and Nexus upload fields to 0.1.3.**
- [ ] **Step 2: Run `dotnet test` and `dotnet build` Release with `InstallAfterBuild=false`; require zero failures, warnings, and errors.**
- [ ] **Step 3: Run `scripts/Package.ps1`, validate the two-file payload, and remove the superseded 0.1.2 ZIP only after validation succeeds.**
- [ ] **Step 4: Confirm Ratopia is closed, back up the installed DLL, install 0.1.3, and compare build/install/ZIP DLL SHA-256 values.**
- [ ] **Step 5: Run a standalone startup log smoke test, restore all other Mod DLLs, and report any game-internal visual checks that remain manual.**
