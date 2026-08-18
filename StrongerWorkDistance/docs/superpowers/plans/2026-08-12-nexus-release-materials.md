# Stronger Work Distance Nexus Release Materials Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Ratopia mod work must remain in the primary agent.

**Goal:** Produce a copy-ready bilingual Nexus Mods release kit and an exact-text 1600×900 cover for Stronger Work Distance 0.1.0.

**Architecture:** Keep Nexus-only text and images under `release/NexusMods`, outside the runtime ZIP. Define a PowerShell contract validator first, then create the text assets and an SVG title card, render the SVG to PNG with headless Microsoft Edge, and validate the release kit against the existing package and README.

**Tech Stack:** PowerShell 7, Nexus BBCode, SVG 1.1, headless Microsoft Edge, .NET `System.Drawing` and `System.IO.Compression`.

## Global Constraints

- English title is exactly `Stronger Work Distance`.
- Chinese title is exactly `更强大的工作距离`.
- Mod version is exactly `0.1.0`; plugin GUID is `cn.ratopia.strongerworkdistance`.
- Main file remains `dist/更强大的工作距离-v0.1.0-BepInEx5.zip`; do not rebuild or alter it.
- Installation path is exactly `BepInEx/plugins/StrongerWorkDistance/StrongerWorkDistance.dll`.
- Cover outputs are exactly 1600×900 SVG and PNG and contain only the two titles as visible text.
- Do not claim that in-save gameplay acceptance, save/reload, or uninstall restoration has passed; list these as pre-publication checks.
- Do not modify the Ratopia installation directory.

---

### Task 1: Add a failing Nexus release contract validator

**Files:**
- Create: `scripts/Test-NexusRelease.ps1`

**Interfaces:**
- Consumes: repository root, `release/NexusMods`, `README.md`, and the existing `dist` ZIP.
- Produces: exit code 0 plus `NEXUS_RELEASE_VALID=True` when every contract passes.

- [ ] **Step 1: Create the validator before release assets exist**

Implement checks for the six required text files, both cover files, exact title/version/path strings, BBCode presence, the 1600×900 PNG dimensions, exact SVG titles/viewBox, and the two expected ZIP entries. Reject placeholders and forbidden DLL/PDB/log/save files.

- [ ] **Step 2: Run the validator and verify RED**

Run: `pwsh -NoProfile -File .\scripts\Test-NexusRelease.ps1`

Expected: non-zero exit with `缺少 Nexus 发布资源` because `release/NexusMods` has not been created.

- [ ] **Step 3: Commit the verified failing contract**

```powershell
git add scripts/Test-NexusRelease.ps1
git commit -m "test: define Nexus release material contracts"
```

### Task 2: Create bilingual Nexus page copy

**Files:**
- Create: `release/NexusMods/NEXUS_TITLE.txt`
- Create: `release/NexusMods/NEXUS_SUMMARY.txt`
- Create: `release/NexusMods/NEXUS_DESCRIPTION.txt`
- Create: `release/NexusMods/FILE_DESCRIPTION.txt`
- Create: `release/NexusMods/CHANGELOG.txt`
- Create: `release/NexusMods/UPLOAD_CHECKLIST.md`

**Interfaces:**
- Consumes: `README.md`, plugin constants, version `0.1.0`, and the exact install path.
- Produces: copy-ready English title and bilingual summary/page/file descriptions.

- [ ] **Step 1: Write the exact page title and a bilingual short summary**

Use `Stronger Work Distance` as the entire title. Keep the combined two-line English/Chinese summary under 250 characters.

- [ ] **Step 2: Write the complete BBCode description**

Provide English first and Chinese second. Include overview, features, scope exclusions, requirements, installation, save compatibility, uninstallation, known conflicts, troubleshooting, and `0.1.0` changelog. State the full 25-position range as horizontal `-2..2` and vertical `+1..-3` in technical wording while explaining it to players as 2 tiles horizontally and up to 4 tiles high.

- [ ] **Step 3: Write file description, changelog, and upload checklist**

The checklist must point to the existing dist ZIP and cover PNG, include the current ZIP SHA-256, and leave gameplay range, boundary, save/reload, and uninstall restoration checks unchecked.

- [ ] **Step 4: Run the validator and confirm only cover checks remain RED**

Run: `pwsh -NoProfile -File .\scripts\Test-NexusRelease.ps1`

Expected: non-zero exit naming `images/StrongerWorkDistance-cover-1600x900.svg` as missing.

- [ ] **Step 5: Commit page copy**

```powershell
git add release/NexusMods
git commit -m "docs: add bilingual Nexus release copy"
```

### Task 3: Create and render the exact-text cover

**Files:**
- Create: `release/NexusMods/images/StrongerWorkDistance-cover-1600x900.svg`
- Create: `release/NexusMods/images/StrongerWorkDistance-cover-1600x900.png`

**Interfaces:**
- Consumes: exact English and Chinese titles.
- Produces: editable SVG and upload-ready 1600×900 PNG.

- [ ] **Step 1: Create the deterministic SVG**

Use a 1600×900 dark navy gradient, a subtle centered 5×5 range grid, cyan/gold accents, and centered title text. Visible text must be only `STRONGER WORK DISTANCE` and `更强大的工作距离`.

- [ ] **Step 2: Render the PNG with headless Edge**

Run Microsoft Edge with `--headless=new --disable-gpu --hide-scrollbars --window-size=1600,900 --screenshot=<absolute PNG path> <absolute SVG file URI>` using a temporary project-local profile.

Expected: a non-empty 1600×900 PNG.

- [ ] **Step 3: Run the validator and verify GREEN**

Run: `pwsh -NoProfile -File .\scripts\Test-NexusRelease.ps1`

Expected: exit 0 with `NEXUS_RELEASE_VALID=True`, `COVER_SIZE=1600x900`, and the existing ZIP SHA-256.

- [ ] **Step 4: Inspect the PNG visually**

Open the PNG and verify both titles are exact, readable, centered, uncropped, and free of mojibake.

- [ ] **Step 5: Commit the cover**

```powershell
git add release/NexusMods/images
git commit -m "art: add bilingual Nexus title cover"
```

### Task 4: Final audit

**Files:**
- Inspect: `release/NexusMods/**`
- Inspect: `scripts/Test-NexusRelease.ps1`

**Interfaces:**
- Produces: an evidence-backed upload handoff without modifying the Mod ZIP or game installation.

- [ ] **Step 1: Run the validator from a clean command**

Run: `pwsh -NoProfile -File .\scripts\Test-NexusRelease.ps1`

Expected: exit 0 and no warnings/errors.

- [ ] **Step 2: Inspect repository state and file inventory**

Run: `git status --short` and list every release file with its byte size. Confirm only the intended committed files exist.

- [ ] **Step 3: Verify the existing ZIP remains byte-identical**

Run: `Get-FileHash .\dist\更强大的工作距离-v0.1.0-BepInEx5.zip -Algorithm SHA256`

Expected: `015B22B2BE375EA3EE62D0E0698DFCED9DB61CA5531EDD18E686AFC6EFEA97F8`.

- [ ] **Step 4: Hand off exact upload paths and the pending gameplay gate**

Report the title/summary/description/file-description/checklist paths, cover PNG path, and main ZIP path. Explicitly state that the unchecked in-game acceptance items must be completed before public release.

## Self-review

The plan covers every design-spec file, bilingual copy, exact title rendering, package immutability, image dimensions, BBCode/content contracts, visual inspection, and the honest pre-publication gameplay gate. It contains no placeholder implementation steps and uses one consistent version, path, and image name throughout.

