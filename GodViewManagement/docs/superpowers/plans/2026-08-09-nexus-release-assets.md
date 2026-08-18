# Nexus Mods Release Assets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce an accurate bilingual Nexus Mods publishing kit and a title-only cover for God View Management v0.1.1.

**Architecture:** Keep all page copy and image assets in a standalone `NexusMods` directory so the existing source, installed DLL, and release ZIP remain unchanged. Author the cover as deterministic SVG, render it to PNG, and verify dimensions and text mechanically.

**Tech Stack:** Markdown, SVG 1.1, PNG, PowerShell verification, and `System.Drawing` for deterministic raster rendering.

## Global Constraints

- English public title is exactly `God View Management`.
- Cover text contains English only and uses no Ratopia game assets.
- Public version is `0.1.1` and tested game version is Ratopia `1.0.0600`.
- Required runtime is BepInEx `5.4.23.5` for the Mono game build.
- Do not change or repackage the plugin DLL.
- The workspace is not a Git repository, so no commit step is possible.

---

### Task 1: Nexus page copy

**Files:**
- Create: `NexusMods/NEXUS_PAGE_BILINGUAL.md`
- Create: `NexusMods/NEXUS_UPLOAD_FIELDS.md`

**Interfaces:**
- Consumes: Current `README.md` behavior and compatibility claims.
- Produces: Copy-paste-ready English and Chinese page text plus upload metadata.

- [x] **Step 1:** Write English-first bilingual page copy with overview, features, controls, requirements, installation, limitations, compatibility, uninstall, troubleshooting, and changelog.
- [x] **Step 2:** Write the concise upload-field checklist using title `God View Management`, version `0.1.1`, category `Miscellaneous`, and truthful tags.
- [x] **Step 3:** Confirm `0.1.0` appears only in the historical changelog, then search both documents for `TBD` and `TODO`; expect no matches.

### Task 2: Title-only cover

**Files:**
- Create: `NexusMods/GodViewManagement-cover.svg`
- Create: `NexusMods/GodViewManagement-cover-1600x900.png`

**Interfaces:**
- Consumes: Exact title from Task 1.
- Produces: A reusable vector source and upload-ready raster cover.

- [x] **Step 1:** Author a 1600x900 dark navy and gold SVG containing only the two-line title `GOD VIEW` / `MANAGEMENT`.
- [x] **Step 2:** Render the same deterministic layout to `GodViewManagement-cover-1600x900.png` without altering the SVG source.
- [x] **Step 3:** Verify the PNG opens, measures exactly 1600x900, and uses 24-bit RGB without transparency.
- [x] **Step 4:** Inspect the rendered image visually for spelling, cropping, contrast, and thumbnail readability.

### Task 3: Release-resource audit

**Files:**
- Verify: `NexusMods/*`
- Verify unchanged: `dist/上帝视角管理-v0.1.1-BepInEx5.zip`

**Interfaces:**
- Consumes: Outputs from Tasks 1 and 2.
- Produces: Final file inventory and evidence that the plugin package was not modified.

- [x] **Step 1:** Record the existing ZIP SHA-256 and compare it with the known value `E8A02F1F29E1C5BB65128EE5A205A831C456CB5670A2FA688666AD74229F4975`.
- [x] **Step 2:** List every file in `NexusMods` and confirm no DLL, save, log, or game asset is present.
- [x] **Step 3:** Report the final paths, cover dimensions, and publication cautions.
