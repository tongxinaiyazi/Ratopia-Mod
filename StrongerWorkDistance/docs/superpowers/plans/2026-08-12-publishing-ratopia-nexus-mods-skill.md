# Publishing Ratopia Nexus Mods Skill Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Ratopia work must remain in the primary agent.

**Goal:** Install and verify a personal Codex Skill that always produces exactly five Ratopia Nexus Mods deliverables with one cover image format.

**Architecture:** Put procedural rules in `SKILL.md` and enforce the output shape with a PowerShell validator. Reuse `developing-ratopia-mods/scripts/Test-RatopiaPackage.ps1` for the Mod ZIP instead of duplicating Ratopia package rules.

**Tech Stack:** Markdown Skill instructions, YAML UI metadata, PowerShell 7, system Skill Creator validation scripts.

## Global Constraints

- Skill name: `publishing-ratopia-nexus-mods`.
- Install root: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods`.
- Final delivery has exactly five files: English title, summary, bilingual full description, one cover, and one Mod ZIP.
- Default cover is PNG; explicit JPG requests switch to JPG. PNG and JPG must never coexist in the final directory.
- Validate the ZIP with the existing Ratopia package validator.
- Do not use subagents for Ratopia work; the real failure in this conversation is the RED baseline.

---

### Task 1: Initialize the personal Skill

**Files:**
- Create: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\SKILL.md`
- Create: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\agents\openai.yaml`
- Create: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1`

- [ ] Run `init_skill.py publishing-ratopia-nexus-mods --path C:\Users\ASUS\.codex\skills --resources scripts` with the approved UI metadata.
- [ ] Confirm the generated template exists and no example or auxiliary files were created.

### Task 2: Implement the failing output-contract tests

**Files:**
- Create: temporary invalid and valid fixtures under the project `artifacts` directory.
- Modify: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\scripts\Test-RatopiaNexusDeliverables.ps1`

- [ ] Create an invalid fixture containing both `4-封面.png` and `4-封面.jpg`; run the validator before implementation and record that the missing script/behavior fails.
- [ ] Implement parameters `-Path`, `-ModName`, and `-Version`, then require exact filenames, exactly five files, non-empty text, one image format, image dimensions of at least 1280×720, and a correctly named ZIP.
- [ ] Delegate ZIP validation to `developing-ratopia-mods/scripts/Test-RatopiaPackage.ps1 -ExpectedPluginName <ModName>` and fail if any package errors exist.
- [ ] Re-run the invalid fixture and require a non-zero exit identifying the extra cover.

### Task 3: Write the minimal workflow Skill

**Files:**
- Modify: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\SKILL.md`
- Verify: `C:\Users\ASUS\.codex\skills\publishing-ratopia-nexus-mods\agents\openai.yaml`

- [ ] Write concise trigger metadata for Ratopia Nexus/N 网 publishing and upload materials.
- [ ] Define the five-file contract, single-image decision rule, content requirements, production order, package validation, and final response format.
- [ ] Explicitly route Mod compatibility and ZIP safety to `developing-ratopia-mods` and prohibit extra final-delivery files.
- [ ] Run `quick_validate.py` against the installed Skill and require success.

### Task 4: Validate with the current Mod

**Files:**
- Create: a temporary correct fixture under `artifacts` using the existing Stronger Work Distance text, PNG, and ZIP.

- [ ] Copy only the three text files, PNG, and ZIP into the fixture using the five canonical names.
- [ ] Run `Test-RatopiaNexusDeliverables.ps1 -ModName StrongerWorkDistance -Version 0.1.0` and require success with exactly five files and cover format PNG.
- [ ] Remove the duplicate JPG from the current `Nexus发布资料` final directory so it follows the new one-format contract.
- [ ] Run the validator on `Nexus发布资料` and require success.
- [ ] Run `quick_validate.py` once more and inspect the installed Skill tree for only `SKILL.md`, `agents/openai.yaml`, and the validator script.

## Self-review

Every design requirement maps to a task. Filenames, Skill paths, validator parameters, single-image logic, and Ratopia package validation are consistent. The plan contains no unresolved placeholders.

