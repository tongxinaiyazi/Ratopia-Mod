# SuperBow Nexus Release Resources Design

## Goal

Create a self-contained `NexusRelease/v0.1.2` directory that the uploader can use to publish SuperBow on the Ratopia Nexus Mods page without repackaging or rewriting content.

## Selected approach

Use an English-first Nexus release kit with one main downloadable ZIP, copy-ready page metadata, a BBCode description, file-page text, changelog, permissions/credits, upload checklist, hashes, and two image assets. This is more useful than shipping only a ZIP and avoids inventing optional files that the mod does not need.

The image set contains:

- the exact 100×100 `WoodBow` texture extracted from Ratopia `resources.assets`;
- a 1280×720 Nexus cover that places a nearest-neighbour enlargement of that exact icon on a simple dark background without redrawing or generatively modifying it.

The user confirmed permission to upload the original game icon. The credits state that the icon and Ratopia remain property of Cassel Games and that the icon is used with permission.

## Release structure

```text
NexusRelease/v0.1.2/
├── README.md
├── metadata.json
├── NEXUS_TITLE.txt
├── NEXUS_SUMMARY.txt
├── NEXUS_DESCRIPTION.txt
├── FILE_DESCRIPTION.txt
├── CHANGELOG.txt
├── CREDITS_AND_PERMISSIONS.md
├── UPLOAD_CHECKLIST.md
├── SHA256SUMS.txt
├── files/
│   └── SuperBow-v0.1.2-BepInEx5.zip
└── images/
    ├── SuperBow-Cover-1280x720.png
    └── WoodBow-Original-100x100.png
```

## Content rules

- English title: `Super Bow - Splash Damage and Bleed for the Queen's Bow`.
- English summary and page body accurately describe version 0.1.2 only.
- Requirements list BepInEx 5 and explicitly state that BepInEx, Harmony, Unity, and game DLLs are not bundled.
- Installation targets `BepInEx/plugins/SuperBow/SuperBow.dll`.
- Save/uninstall notes explain the vanilla `RangeAtk=1` and `BloodDrain=3` markers.
- Compatibility locks Unity `2021.3.21f1`, BepInEx `5.4.23.5`, Harmony `2.9.0.0`, and the inspected Assembly-CSharp hash.
- The upload checklist does not claim gameplay verification beyond available evidence.
- The upload checklist records AI assistance and warns not to use the 2026 Nexus anniversary-event tag, whose rules disallow generative AI.
- The mod ZIP remains byte-for-byte identical to the already validated 0.1.2 package and does not contain the cover or extracted game icon.

## Validation

- Confirm the copied ZIP hash matches the original distribution ZIP.
- Re-run the Ratopia package audit against the copied ZIP.
- Confirm the ZIP contains only `README.md` and `BepInEx/plugins/SuperBow/SuperBow.dll`.
- Confirm cover dimensions are 1280×720 and original icon dimensions are 100×100.
- Confirm required copy files are non-empty, version strings agree, and `metadata.json` parses.
- Visually inspect both PNG files.

## Self-review

The design contains no placeholders, does not add optional binaries, does not place the copyrighted icon inside the downloadable mod archive, and distinguishes build/package verification from pending user gameplay acceptance.
