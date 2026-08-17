# Standalone Unlimited Trade Agreements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia Mod work must remain in the primary agent.

**Goal:** Build a standalone BepInEx 5 package that removes only Ratopia's trade-agreement count limit and expands the vanilla trade-agreement UI.

**Architecture:** A small pure rules class owns slot-count and label behavior. Harmony adapters patch only `IsFullTradeAgreement`, `DiplomaticTradeLayoutUI.UpdateSlot`, and `DiplomaticWorldDetailUI.Refresh`; all other optimization features remain absent.

**Tech Stack:** C#/.NET Framework 4.7.2, BepInEx 5.4.23.5, Harmony 2.9.0, xUnit, Mono.Cecil, PowerShell.

## Global Constraints

- Plugin name is `贸易站去除最大队列限制`, GUID is `cn.ratopia.unlimitedtradeagreements`, version is `0.1.0`.
- Target Assembly-CSharp SHA-256 is `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- The package must not contain research, full-resource-pool, preview, agreement-editing, infinite-period, or price-refresh behavior.
- The plugin must declare incompatibility with `cn.ratopia.unlimitedresearchandtradequeues`.
- Build and package only; do not install into Ratopia.

---

### Task 1: Scaffold the independent contract tests

**Files:**
- Create: `UnlimitedTradeAgreements/UnlimitedTradeAgreements.sln`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/UnlimitedTradeAgreements.Tests.csproj`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/CoreRulesTests.cs`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/TranspilerTests.cs`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/PluginContractTests.cs`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/GameContractTests.cs`
- Create: `UnlimitedTradeAgreements/tests/UnlimitedTradeAgreements.Tests/ReleaseContractTests.cs`

**Interfaces:**
- Consumes: game and BepInEx assemblies from `RatopiaDir`.
- Produces: failing tests that require `TradeQueueRules`, `TradeLayoutTranspiler`, plugin metadata, exact patch targets, and the release ZIP.

- [ ] Write tests for `GetVisibleSlotCount(int)`, `GetUnlimitedCountLabel(int)`, the single fixed-7 IL rewrite, plugin metadata, forbidden patch types, game contracts, and ZIP contents.
- [ ] Run `dotnet test .\UnlimitedTradeAgreements.sln -c Release /p:RatopiaDir=E:\steam\steamapps\common\Ratopia /p:InstallAfterBuild=false` and verify failure because the standalone plugin DLL/types do not yet exist.

### Task 2: Implement the minimum standalone plugin

**Files:**
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/UnlimitedTradeAgreements.csproj`
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/Plugin.cs`
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/Core/TradeQueueRules.cs`
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/Runtime/TradeQueueRuntime.cs`
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/Patching/TradeLayoutTranspiler.cs`
- Create: `UnlimitedTradeAgreements/src/UnlimitedTradeAgreements/Patches/TradeQueuePatches.cs`

**Interfaces:**
- Produces: `TradeQueueRules.GetVisibleSlotCount(int)`, `TradeQueueRules.GetUnlimitedCountLabel(int)`, and `TradeLayoutTranspiler.Rewrite(IEnumerable<CodeInstruction>)`.

- [ ] Implement rules returning `Math.Max(7, count)` and `$"{count}/∞"`.
- [ ] Implement a structured transpiler that replaces exactly one `ldc.i4.7` followed by `blt`/`blt.s`; throw on zero or multiple matches.
- [ ] Install only three Harmony patch classes and unpatch all if any installation fails.
- [ ] Run targeted tests and verify all Task 1 behavior tests pass.

### Task 3: Add documentation and deterministic packaging

**Files:**
- Create: `UnlimitedTradeAgreements/README.md`
- Create: `UnlimitedTradeAgreements/scripts/Package.ps1`

**Interfaces:**
- Produces: `UnlimitedTradeAgreements/dist/贸易站去除最大队列限制-v0.1.0-BepInEx5.zip`.

- [ ] Document scope, incompatibility, installation, uninstall, save risk, and log locations in Chinese.
- [ ] Package only `BepInEx/plugins/UnlimitedTradeAgreements/UnlimitedTradeAgreements.dll` and `README.md`.
- [ ] Run the full Release test suite with `InstallAfterBuild=false`.
- [ ] Run the package script, verify archive structure and forbidden DLL scan, and record DLL/ZIP SHA-256 hashes.
