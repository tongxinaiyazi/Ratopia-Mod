# Standalone Runtime Driver Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make God View Management initialize and run without depending on the presence or timing side effects of any other Mod.

**Architecture:** Protect the shared BepInEx host in this plugin's own `Awake`, retain the `TileMgr.Update` Harmony postfix, add a `BaseUnityPlugin.Update` entry point, and route both through a once-per-Unity-frame gate. Add contract coverage for lifecycle protection and the standalone entry point, then update release metadata/package/install artifacts to 0.1.2.

**Tech Stack:** C# / net472, BepInEx 5, Harmony 2.9, Unity 2021.3, xUnit, Mono.Cecil, PowerShell.

## Global Constraints

- Game directory: `E:\steam\steamapps\common\Ratopia`.
- Locked `Assembly-CSharp.dll` SHA-256: `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Do not add save fields or cross-Mod APIs.
- Runtime must execute at most once per Unity frame.
- `HideAndDontSave` and `DontDestroyOnLoad` must be applied before `Plugin.Instance` is published.
- Installation is allowed only while Ratopia is not running and must preserve a backup of the replaced DLL.
- The final distribution may contain only the plugin DLL and README payload.

---

### Task 1: Regression tests for independent runtime driving

**Files:**
- Create: `tests/GodViewManagement.Tests/RuntimeTickGateTests.cs`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`

**Interfaces:**
- Produces: `RuntimeTickGate.TryEnter(int frameCount) : bool` contract.
- Produces: private Unity message `Plugin.Update() : void` contract.

- [ ] **Step 1: Write tests that require first-source acceptance, same-frame rejection, next-frame acceptance, and a parameterless `Plugin.Update`.**
- [ ] **Step 2: Run the focused tests and verify RED because `RuntimeTickGate` and `Plugin.Update` do not exist.**
- [ ] **Step 3: Record the exact failure count and messages.**

### Task 2: Minimal dual-driver implementation

**Files:**
- Create: `src/GodViewManagement/Core/RuntimeTickGate.cs`
- Modify: `src/GodViewManagement/Plugin.cs`

**Interfaces:**
- Consumes: `RuntimeTickGate.TryEnter(int frameCount) : bool`.
- Produces: one shared guarded runtime entry used by both `Plugin.Update` and `TickFromTileManager`.

- [ ] **Step 1: Implement a last-frame gate initialized outside Unity's nonnegative frame range.**
- [ ] **Step 2: Add `Plugin.Update`, resolve `GameMgr.Instance?._TileMgr`, and route it to the shared driver.**
- [ ] **Step 3: Route the Harmony callback to the same driver and log the first accepted driver source once.**
- [ ] **Step 4: Run the focused tests and verify GREEN.**
- [ ] **Step 5: Run the complete Release test suite and build with `InstallAfterBuild=false`.**

### Task 2A: Independent BepInEx host lifetime

**Files:**
- Modify: `src/GodViewManagement/Plugin.cs`
- Modify: `tests/GodViewManagement.Tests/PluginContractTests.cs`

**Interfaces:**
- Produces: lifecycle protection applied before `Plugin.Instance` is assigned.

- [ ] **Step 1: Add a Mono.Cecil contract test requiring `HideAndDontSave` value 61 and `DontDestroyOnLoad` before `set_Instance`; verify RED.**
- [ ] **Step 2: Apply both protections at the beginning of `Awake`; verify the focused lifecycle and dual-driver tests are GREEN.**

### Task 3: Versioned release, package, and local installation

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
- Produces: plugin and package version `0.1.2`.
- Produces: `dist/上帝视角管理-v0.1.2-BepInEx5.zip`.

- [ ] **Step 1: Update all source, test, README, Nexus, and package version references to 0.1.2 and document the standalone-driver fix.**
- [ ] **Step 2: Run all Release tests and build with installation disabled.**
- [ ] **Step 3: Package and validate that only the DLL and README payload are present.**
- [ ] **Step 4: Remove the superseded 0.1.1 distribution only after 0.1.2 package validation succeeds.**
- [ ] **Step 5: Confirm Ratopia is closed, back up the installed DLL, install 0.1.2, and compare build/install/ZIP DLL SHA-256 values.**
- [ ] **Step 6: Report automated evidence and keep the game-internal acceptance items explicitly marked as manual if no controlled standalone gameplay run was performed.**
