# Standalone Runtime Driver Design

## Problem

God View Management 0.1.1 installs successfully, but a user reports that the HUD and controls do not become available when it is the only loaded Mod. Adding Shared Warehouse appears to make it work.

The supplied log proves plugin discovery and successful `Awake`, but it contains no session-ready or mode-toggle message. It ends four seconds after `Loaded Game`, while a known working local session reached God View session readiness about five seconds after `Loaded Game`.

Shared Warehouse does not call God View Management and does not patch `TileMgr.Update`. Inspection of the installed Shared Warehouse DLL revealed the actual coupling: its `Awake` sets the shared BepInEx host `GameObject` to `HideAndDontSave` (numeric value 61) and calls `DontDestroyOnLoad` before initializing its own state. God View Management 0.1.1 did neither. When Ratopia cleaned the BepInEx host during startup/scene transition, God View Management received `OnDestroy`, cleared `Instance`, and its already-installed Harmony callback no longer had a live runtime to call. Adding Shared Warehouse accidentally protected the shared host for both plugins.

God View Management also had no plugin-owned update loop and depended entirely on the `TileMgr.Update` Harmony postfix for session discovery, HUD creation, hotkeys, camera control, and remote clicks. This was a second avoidable single point of failure, although not the primary cross-Mod dependency.

## Decision

At the start of God View Management `Awake`, independently protect the shared BepInEx host with `gameObject.hideFlags |= HideFlags.HideAndDontSave` and `DontDestroyOnLoad(gameObject)`. This happens before publishing `Plugin.Instance`, so a partially initialized plugin is never exposed.

Keep the existing `TileMgr.Update` postfix as a stable game-owned driver and add a self-owned `Plugin.Update` driver. Both feed one guarded runtime entry point. A pure frame gate accepts only the first source in a Unity frame, so input and camera logic cannot execute twice.

The plugin update path obtains the current tile manager from `GameMgr.Instance?._TileMgr`. It does not cache a scene object, add a save field, or require Shared Warehouse. The Harmony path remains active for resilience if Unity does not invoke the plugin component first.

The first accepted driver invocation is logged once at Info level. The existing session-ready log remains the proof that all required game managers are ready. Together these messages distinguish plugin discovery, runtime driving, and session readiness in future user logs.

## Safety and compatibility

- Mode still defaults to off on startup and session changes.
- Host protection is owned by God View Management and no longer supplied accidentally by Shared Warehouse.
- The runtime continues to reject loading sessions and incomplete manager graphs.
- Existing queen-input isolation and remote-action guards are unchanged.
- No save data or public cross-Mod API is added.
- Runtime exceptions still trigger the existing fail-safe reset.
- Release version becomes `0.1.2` and the old `0.1.1` distribution is superseded.
