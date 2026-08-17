# Wire Through Walls Nexus Upload Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a complete unpublished Nexus Mods draft for Ratopia with an original bilingual cover and the verified Wire Through Walls v0.1.0 release archive, stopping before public publication.

**Architecture:** Treat the cover, release archive and Nexus page as separate gated artifacts. Generate and visually validate the cover locally, revalidate the immutable release ZIP, then use the user's authenticated Chrome profile to create and review an unpublished Nexus draft. Public publication remains blocked until explicit action-time confirmation and manual gameplay validation.

**Tech Stack:** OpenAI built-in image generation, Ratopia BepInEx 5 package validator, SHA-256, Nexus Mods web UI through the Codex Chrome extension.

## Global Constraints

- Game: `Ratopia`.
- Mod name: `Wire Through Walls - 电线可穿墙`.
- Version: `0.1.0` everywhere.
- Main file: `dist/电线可穿墙-v0.1.0-BepInEx5.zip`.
- Source code is not uploaded.
- Page copy is English first with Simplified Chinese below it.
- Category is `Miscellaneous`; adult content is `No`; Donation Points are enabled.
- Modifications and translations are allowed with credit; unapproved reuploads, commercial use and paid-mod use are prohibited.
- The AI-generated cover is tagged `AI-Generated Content`.
- Do not upload game, Unity, BepInEx, Harmony, PDB, test, log or save files.
- Do not publish the page publicly in this execution; stop for explicit confirmation after the complete draft is reviewed.
- The workspace is not a Git repository, so plan artifact commit steps are not applicable.

---

### Task 1: Generate and validate the Nexus cover

**Files:**
- Create: `assets/nexus/wire-through-walls-cover.png`
- Reference: `docs/superpowers/specs/2026-08-09-nexus-release-design.md`

**Interfaces:**
- Consumes: approved cover direction and exact bilingual title text from the release design.
- Produces: one project-local 16:9 PNG ready for Nexus image upload.

- [ ] **Step 1: Generate the original cover**

Use the built-in image generation tool with this exact production prompt:

```text
Use case: ads-marketing
Asset type: Nexus Mods cover image for a Ratopia utility mod
Primary request: Create an original retro-industrial blueprint cutaway showing one glowing electrical cable passing cleanly through a thick wood-and-stone wall, immediately communicating that wires can coexist with walls and buildings.
Scene/backdrop: handcrafted workshop blueprint backdrop with subtle grid lines, brass drafting tools and warm wood textures; no game screenshots.
Style/medium: polished stylized digital illustration, cozy steampunk construction mood, original artwork that does not imitate or copy Ratopia characters, logos, UI or official promotional art.
Composition/framing: 16:9 landscape, wall cutaway centered, cable crossing left-to-right, strong silhouette and readable at small thumbnail size, generous clean title area in the upper third.
Lighting/mood: warm amber workshop lighting with an electric cyan cable glow; inviting and practical.
Color palette: dark navy blueprint, warm brown wood, gray stone, brass accents, cyan electricity.
Text (verbatim): "WIRE THROUGH WALLS" and "电线可穿墙"
Constraints: render both text lines exactly; English is the large primary title, Simplified Chinese is the smaller subtitle; no other text, no logos, no watermark, no characters, no copyrighted game assets.
Avoid: clutter, tiny technical labels, illegible Chinese, photorealism, official Ratopia branding, UI screenshots.
```

Expected: one landscape cover with both exact title lines and a clear wall/cable concept.

- [ ] **Step 2: Inspect the generated cover**

Check the rendered image at full size for all of these conditions:

- Both title lines are spelled exactly.
- The cable visibly passes through the wall instead of stopping at it.
- No official Ratopia logo, character, screenshot or UI appears.
- No watermark or extra generated text appears.
- The central concept and English title remain readable at thumbnail scale.

Expected: all five checks pass. If only one visual defect exists, perform one targeted image edit and re-inspect.

- [ ] **Step 3: Save the selected cover inside the project**

Copy the selected generated PNG to:

```text
D:\SOFTWARE\项目\鼠托邦mod\WireThroughWalls\assets\nexus\wire-through-walls-cover.png
```

Expected: a non-empty PNG at the exact path, with the final image viewable from the workspace.

### Task 2: Revalidate the upload archive

**Files:**
- Verify: `dist/电线可穿墙-v0.1.0-BepInEx5.zip`

**Interfaces:**
- Consumes: the already-built v0.1.0 archive.
- Produces: fresh package-validator result, archive file size and SHA-256 evidence for the upload review.

- [ ] **Step 1: Run the Ratopia package validator**

Run:

```powershell
& 'C:\Users\ASUS\.codex\skills\developing-ratopia-mods\scripts\Test-RatopiaPackage.ps1' `
  -Path '.\dist\电线可穿墙-v0.1.0-BepInEx5.zip' `
  -ExpectedPluginName 'WireThroughWalls'
```

Expected: `Errors` is empty, `ForbiddenFiles` is empty, and the only plugin DLL is `BepInEx/plugins/WireThroughWalls/WireThroughWalls.dll`.

- [ ] **Step 2: Record the immutable upload identity**

Run:

```powershell
Get-Item -LiteralPath '.\dist\电线可穿墙-v0.1.0-BepInEx5.zip' |
  Select-Object FullName, Length, LastWriteTime
Get-FileHash -LiteralPath '.\dist\电线可穿墙-v0.1.0-BepInEx5.zip' -Algorithm SHA256
```

Expected SHA-256: `2E915C96A9CE1BC07FFA78332BFAC23925162239663F639DC674FA095C5AE239`.

### Task 3: Connect to Nexus Mods and create the draft shell

**Files:**
- Read: `docs/superpowers/specs/2026-08-09-nexus-release-design.md`

**Interfaces:**
- Consumes: the user's existing Chrome profile and approved page metadata.
- Produces: an unpublished Ratopia mod draft with a stable edit page.

- [ ] **Step 1: Connect to the existing Chrome profile**

Initialize the Chrome extension browser connection, read the complete browser documentation, list tabs, and select the user's Nexus Mods tab when present. If no Nexus tab exists, open `https://www.nexusmods.com/games/ratopia` in a new tab.

Expected: Nexus Mods is visible in the user's own Chrome profile.

- [ ] **Step 2: Verify authentication without handling credentials**

Inspect the Nexus page for the logged-in account controls. If Nexus requests credentials, MFA, CAPTCHA or a fresh login, stop and ask the user to complete it directly.

Expected: the account is already authenticated, or control is handed to the user before credential entry.

- [ ] **Step 3: Start a new Ratopia mod page**

Open the Ratopia `Add a mod` flow and enter:

```text
Name: Wire Through Walls - 电线可穿墙
Summary: Allows normal electrical wires to share tiles with walls, roads and other buildings while preserving blueprint, construction, demolition and save-loading behavior.
Version: 0.1.0
Category: Miscellaneous
Adult content: No
```

Expected: the page remains a draft/unpublished entry; no public publish control is activated.

### Task 4: Populate media, description, permissions and requirements

**Files:**
- Upload: `assets/nexus/wire-through-walls-cover.png`
- Read: `docs/superpowers/specs/2026-08-09-nexus-release-design.md`

**Interfaces:**
- Consumes: the draft from Task 3, validated cover and approved copy.
- Produces: a complete unpublished description/media/metadata draft.

- [ ] **Step 1: Upload the cover**

Upload `assets/nexus/wire-through-walls-cover.png` as the main image and set the AI-generated-content tag when the UI exposes it.

Expected: the image preview shows the correct cover and both title lines remain legible.

- [ ] **Step 2: Insert the approved bilingual description**

Copy the complete English and Simplified Chinese description from `docs/superpowers/specs/2026-08-09-nexus-release-design.md`, preserving headings, lists and code paths.

Expected: English appears first, Simplified Chinese second, and no design-only notes or upload sequence text is included.

- [ ] **Step 3: Configure requirements and tags**

Add `BepInEx 5` as an off-site requirement when supported, then select available tags from this ordered set:

```text
Gameplay
Quality of Life
Utilities for Players
AI-Generated Content
```

Expected: only relevant available tags are selected; AI-generated cover disclosure is present.

- [ ] **Step 4: Configure permissions and Donation Points**

Set permissions so that modifications and translations are allowed with credit, reuploads require explicit permission, commercial/paid-mod use is prohibited, and conversions to other games require permission. Enable Donation Points.

Expected: permission summaries match the approved design and Donation Points shows enabled.

### Task 5: Upload the main file

**Files:**
- Upload: `dist/电线可穿墙-v0.1.0-BepInEx5.zip`

**Interfaces:**
- Consumes: the validated archive and Nexus draft.
- Produces: one main downloadable v0.1.0 file attached to the unpublished draft.

- [ ] **Step 1: Upload the archive**

Upload the exact validated ZIP with:

```text
Display name: Wire Through Walls v0.1.0
Version: 0.1.0
Category: Main files
Description: Initial release. Requires BepInEx 5. Extract directly into the Ratopia game directory.
```

Expected: Nexus completes the upload and shows one main file with the correct name, version and size; no source archive is added.

- [ ] **Step 2: Review the uploaded filename and scanner state**

Confirm that the site lists the intended ZIP, not README, DLL, source tree, log, save or dependency files. Record any virus-scan pending state without treating it as a failure.

Expected: exactly one main release archive is associated with v0.1.0.

### Task 6: Final draft review and publication stop

**Files:**
- Verify: Nexus draft page
- Verify: `docs/superpowers/specs/2026-08-09-nexus-release-design.md`

**Interfaces:**
- Consumes: the fully populated unpublished draft.
- Produces: a review report and an explicit action-time confirmation request; does not publish.

- [ ] **Step 1: Review every public-facing field**

Compare the rendered draft to the design and verify:

- Name, summary and version are correct.
- Cover is the selected original image.
- English and Chinese descriptions are complete.
- BepInEx 5 requirement, relevant tags and AI disclosure are present.
- Permissions and Donation Points match the user's choices.
- One validated v0.1.0 main archive is attached.
- No source, logs, saves, PDBs or dependency DLLs were uploaded.

Expected: no mismatches remain.

- [ ] **Step 2: Stop before public publication**

Do not click the final Publish/Submit control. Report the draft state, any site warnings or pending scans, and the remaining manual gameplay gate. Ask for explicit action-time confirmation before making the page public.

Expected: the Nexus draft is complete but unpublished.
