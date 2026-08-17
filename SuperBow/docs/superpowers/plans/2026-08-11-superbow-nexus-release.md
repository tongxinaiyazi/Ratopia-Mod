# SuperBow Nexus Release Resources Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. The user explicitly requested no subagents, so execute sequentially in the primary agent.

**Goal:** Produce a validated, copy-ready Nexus Mods release kit for SuperBow 0.1.2 with English page content, the existing release ZIP, and a cover built from Ratopia's original WoodBow icon.

**Architecture:** Keep Nexus-only materials outside the runtime package in `NexusRelease/v0.1.2`. Extract the icon deterministically from `resources.assets`, compose the cover without generative modification, and validate files, hashes, image dimensions, and ZIP contents with a PowerShell contract script.

**Tech Stack:** PowerShell 7, Python 3.12, UnityPy, Pillow, JSON, Markdown, Nexus BBCode, existing Ratopia package audit script.

## Global Constraints

- Work only in the primary agent; do not use subagents.
- Mod version is exactly `0.1.2`.
- Preserve the validated mod ZIP byte-for-byte.
- Do not add game, Unity, BepInEx, Harmony, logs, saves, PDBs, or source code to the downloadable ZIP.
- Use the exact `WoodBow` 100×100 Texture2D from `Ratopia_Data/resources.assets`, path ID `4255`.
- The user confirmed permission to upload the original icon; credit Cassel Games.
- Do not use generative image tools or redraw the icon.

---

### Task 1: Create copy-ready Nexus metadata and page text

**Files:**
- Create: `NexusRelease/v0.1.2/README.md`
- Create: `NexusRelease/v0.1.2/metadata.json`
- Create: `NexusRelease/v0.1.2/NEXUS_TITLE.txt`
- Create: `NexusRelease/v0.1.2/NEXUS_SUMMARY.txt`
- Create: `NexusRelease/v0.1.2/NEXUS_DESCRIPTION.txt`
- Create: `NexusRelease/v0.1.2/FILE_DESCRIPTION.txt`
- Create: `NexusRelease/v0.1.2/CHANGELOG.txt`
- Create: `NexusRelease/v0.1.2/CREDITS_AND_PERMISSIONS.md`
- Create: `NexusRelease/v0.1.2/UPLOAD_CHECKLIST.md`

**Produces:** English Nexus page assets with fixed version, compatibility, installation, save, conflict, troubleshooting, permission, and AI-assistance disclosures.

- [x] Write the nine copy-ready text files with no placeholders.
- [x] Confirm every version string is `0.1.2` and every install path is `BepInEx/plugins/SuperBow/SuperBow.dll`.
- [x] Parse `metadata.json` with `Get-Content -Raw | ConvertFrom-Json`.

### Task 2: Extract the original icon and create the cover

**Files:**
- Create: `NexusRelease/v0.1.2/images/WoodBow-Original-100x100.png`
- Create: `NexusRelease/v0.1.2/images/SuperBow-Cover-1280x720.png`

**Consumes:** `E:/steam/steamapps/common/Ratopia/Ratopia_Data/resources.assets`, Texture2D path ID `4255`.

**Produces:** Exact source texture and deterministic Nexus cover.

- [x] Use UnityPy to read Texture2D `4255` and verify its name is `WoodBow` and size is 100×100.
- [x] Save the decoded RGBA texture as `WoodBow-Original-100x100.png`.
- [x] Create a 1280×720 dark-background PNG with the icon enlarged to 500×500 using nearest-neighbour resampling and centered without text or redraw.
- [x] Inspect both PNG files visually.

### Task 3: Copy the release ZIP and generate hashes

**Files:**
- Copy: `dist/超级弓箭.zip` → `NexusRelease/v0.1.2/files/SuperBow-v0.1.2-BepInEx5.zip`
- Create: `NexusRelease/v0.1.2/SHA256SUMS.txt`

**Produces:** Nexus main file identical to the validated distribution file.

- [x] Copy the ZIP without recompressing it.
- [x] Compare source and copied ZIP SHA-256 values and stop on mismatch.
- [x] Record hashes for the ZIP, DLL inside the build output, cover, and original icon.

### Task 4: Add and run the release-kit validator

**Files:**
- Create: `scripts/Test-NexusRelease.ps1`

**Produces:** Repeatable validation of future Nexus release kits.

- [x] Validate the nine text files, two PNG files, ZIP, and hash manifest exist and are non-empty.
- [x] Validate JSON parsing, version consistency, image dimensions, and exact ZIP entries.
- [x] Validate the copied ZIP hash matches `dist/超级弓箭.zip`.
- [x] Run `Test-RatopiaPackage.ps1` against the copied ZIP and require zero errors.
- [x] Run `scripts/Test-NexusRelease.ps1` and require exit code 0.

### Task 5: Final release audit

**Files:**
- Inspect: `NexusRelease/v0.1.2/**`

**Produces:** Final handoff with upload-ready paths and verified hashes.

- [x] Search the release kit for placeholders, local save paths, game DLLs, logs, and PDB references in packaged files.
- [x] List the final tree and file sizes.
- [x] Re-run the validator after all edits.
- [x] Report that static/package/image validation passed, while keeping gameplay acceptance status separate.

## Self-review

All design requirements map to a task. File paths and version strings are consistent, the icon stays outside the mod ZIP, and no task requires unavailable author-account information or a subagent.
