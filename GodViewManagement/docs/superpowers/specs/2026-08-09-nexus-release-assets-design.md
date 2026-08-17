# Nexus Mods Release Assets Design

## Goal

Create a small, usable Nexus Mods publishing kit for God View Management v0.1.1 without changing the plugin or its release ZIP.

## Deliverables

- English mod name: `God View Management`.
- A 1600x900 title-only PNG cover using a dark navy background, restrained gold accents, and the exact English mod name.
- One bilingual Nexus page document with English first and Chinese second.
- One upload-field checklist covering title, summary, version, category, tags, requirements, file name, and file description.

## Content Rules

- Describe only implemented behavior documented by the current README.
- State Ratopia 1.0.0600 and BepInEx 5.4.23.5 as the tested environment.
- Explain that remote configuration excludes Queen-only repair, demolition, delivery, and special interactions.
- Do not claim full compatibility with future game versions.
- Do not include game screenshots, game artwork, logos, or third-party assets in the cover.
- Do not tag the release for the 2026 Nexus Mods 25th Anniversary Mod Drive.

## Visual Design

The cover is a deterministic typographic graphic, not generated artwork. It contains only `GOD VIEW` and `MANAGEMENT`, centered in two lines. A subtle grid and border suggest top-down management while keeping the title legible at thumbnail size.

## Verification

- Confirm the PNG is exactly 1600x900 and opens successfully.
- Confirm the visible title is spelled exactly `GOD VIEW MANAGEMENT`.
- Confirm `0.1.0` appears only as the historical changelog heading and scan for unfinished placeholders.
- Confirm all Nexus resources are outside the distributable ZIP.
