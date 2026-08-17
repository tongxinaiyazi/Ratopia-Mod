# 装备重铸自选属性 Implementation Plan

> **For agentic workers:** Use test-driven development for every behavior change. Do not install while Ratopia is running. Keep game/runtime references out of build and release output.

**Goal:** Build a standalone Ratopia BepInEx 5 mod that lets players choose the original valid reforging attribute at the Royal Blacksmith (level 1) and Hell Anvil (level 2), while preserving vanilla costs, values, achievements, prompts, and save data.

**Architecture:** Pure C# core types resolve paired candidates and session selection without game dependencies. Runtime adapters create a Unity UI panel and temporarily narrow the matching `ItemEnhanceInfo` candidate lists during `T_Queen.ItemEnhance`, restoring original references in a Harmony finalizer.

**Tech Stack:** C# / .NET Framework 4.7.2, BepInEx 5.4.23.5, Harmony 2.9.0, Unity UI, TextMeshPro, xUnit, Mono.Cecil.

## Global Constraints

- Project folder: `D:\SOFTWARE\项目\鼠托邦mod\EquipmentReforgeSelector`.
- Game folder: `E:\steam\steamapps\common\Ratopia`.
- Plugin name: `装备重铸自选属性`; GUID: `cn.ratopia.equipmentreforgeselector`; version: `0.1.0`.
- Target: BepInEx 5 Mono and `net472`; all game, Unity, BepInEx, and Harmony references use `Private="false"`.
- Inspected `Assembly-CSharp.dll` SHA-256: `C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D`.
- Only original candidates for `ItemEnhanceInfo.Type == ItemInfo.m_Type`; exclude the current same-level ability.
- Preserve vanilla materials, values, prompts, achievements, and `T_Queen.Dic_ItemPlusEffect` save schema.
- Invalid selection/data falls back to vanilla random with normal vanilla resource consumption and a visible/logged warning.
- Do not modify existing mods or package any game, Unity, BepInEx, Harmony, Mono.Cecil, test, log, save, PDB, `bin`, or `obj` files.

---

### Task 1: Pure selection and scoped override logic

Create the solution and test project, then use red-green TDD to implement candidate pairing/filtering, per-item/per-level selection sessions, stale-selection replacement, and reference-safe scoped list overrides with idempotent and nested restoration.

### Task 2: Ratopia runtime integration and UI

Use red-green TDD and static contracts to add the plugin, diagnostic patch installation, runtime game adapter, `BuildMidUI.ItemDetail_Open` panel activation, Unity UI selector, and `T_Queen.ItemEnhance` candidate narrowing with Postfix/Finalizer cleanup. Add Mono.Cecil contracts for the exact game build and signatures.

### Task 3: Documentation, packaging, and release validation

Add Chinese README/testing documentation and a PowerShell package script. Run the full Release tests/build without installation, validate release output dependencies and ZIP structure, then prepare the exact installable artifact.

### Task 4: Installation and game acceptance

With Ratopia closed, back up any existing target plugin and the selected test save, install only the mod DLL, compare SHA-256, then verify discovery, patching, first UI invocation, both smith levels, all equipment categories, material/value preservation, save/reload, and vanilla readability after temporary removal.

