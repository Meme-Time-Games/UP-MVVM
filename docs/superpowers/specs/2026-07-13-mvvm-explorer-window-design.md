# MVVM Explorer Window — Design

Date: 2026-07-13
Package: `UP-MVVM` (`com.custom.mvvm`)
Status: Approved, pending implementation plan

## Problem

The package ships two editor windows, `Tools/MVVM/Event Searcher`
([EventViewModelReferencesEditorWindow](../../../Editor/EventViewModel/EventViewModelReferencesEditorWindow.cs)) and
`Tools/MVVM/Reactive Searcher`
([ReactiveVariableReferencesEditorWindow](../../../Editor/ReactiveVariables/ReactiveVariableReferencesEditorWindow.cs)).

They are near-duplicates of each other and answer only one question — "who references the asset I already have
selected?" — while carrying several defects:

- The reactive window opens with the window title `"Event Searcher"` (copy-paste leftover).
- Both call `AssetDatabase.LoadMainAssetAtPath` on **every asset path in the project**, then re-serialize every
  component of every loaded GameObject. This freezes the editor for seconds on each search.
- Both only inspect `Component`s on GameObjects and prefabs. References held by **other ScriptableObjects** — which is
  how most `Installer` wiring works — are invisible.
- The event window swallows all exceptions in an empty `catch { continue; }`.
- Both are driven by `Selection.activeObject`, so they are useless until the user has already located the asset.

The main game holds roughly **1000** `EventViewModelSO` and `ReactiveVariableSO` assets. At that scale, four distinct
pains were confirmed with the user, all of them real:

1. **Discovery** — does an event for this already exist, or am I about to create a duplicate?
2. **Tracing** — who raises this, and who listens to it?
3. **Runtime debugging** — what is this variable's value right now, and did this event actually fire?
4. **Authoring** — create a new SO of the right type in the right folder without digging through the Create menu.

## Solution

A single editor window, **`Tools/MVVM/Explorer`**, that serves all four. The two existing searcher windows are deleted.

The four pains are four things done to *one* SO asset, so they belong behind one window: discovery feeds tracing feeds
runtime debugging without the user ever leaving it.

### Non-goals (v1)

- **Distinguishing raisers from listeners.** A GUID scan proves *that* an asset references the SO, not the role it
  plays. Deriving the role means mapping the referencing installer type (e.g.
  `RaiseEventViewModelControllerInstaller` → raiser) to a semantic. Deferred until the index is proven in use.
- **Rename / safe-delete refactoring.** Read-only tracing first.
- **Replacing the two custom inspectors.** [EventViewModelEditor](../../../Editor/EventViewModel/EventViewModelEditor.cs)
  and [ReactiveVariableEditor](../../../Editor/EventViewModel/ReactiveVariableEditor.cs) stay. Only their
  "Open References Searcher" button is retargeted to open the Explorer focused on the inspected asset.

## Architecture

The window is a shell. Each unit of work is a separate class with one reason to change, so no god-window forms and the
non-Unity-facing logic stays testable.

| Type | Responsibility |
| --- | --- |
| `MvvmExplorerWindow` | `EditorWindow` shell. Composes panels. Owns no logic. |
| `MvvmAssetRepository` | Enumerates all `EventViewModelSO` and `BaseReactiveVariableSO` assets via `AssetDatabase.FindAssets("t:…")`. |
| `ReferenceIndex` | Holds `guid → referencing asset paths`. Pure lookup. Performs no scanning. |
| `ReferenceIndexBuilder` | Builds `ReferenceIndex` by scanning YAML. Persists the result under `Library/`. |
| `ReferenceIndexUpdater` | `AssetPostprocessor`. Patches the index for changed assets only. |
| `OpenSceneReferenceScanner` | Walks components in currently-loaded scenes to catch unsaved edits the YAML scan cannot see. |
| `EventRaiseRecorder` | Play-mode observer. Records raises of watched events. Disposed on play-mode exit. |
| `MvvmAssetFactory` | `Create` — instantiates and saves a new SO asset of a chosen concrete type. |
| `AssetListPanel` / `ReferencesPanel` / `RuntimePanel` | Draw only. One panel per pane. |

### The reference index

This is the core of the design and the reason the new window is fast where the old ones are not.

To find what references an SO, take the SO's **GUID** and text-scan the raw YAML of `.prefab`, `.unity`, and `.asset`
files for it. No asset is loaded, nothing is deserialized.

Three consequences:

1. It is fast enough to build for the whole project in one pass.
2. It catches **ScriptableObject → ScriptableObject** references, which the current windows miss entirely.
3. The result is a plain map, so it can be cached and incrementally patched.

The index is built once, cached to `Library/`, and thereafter patched by `ReferenceIndexUpdater` as assets change. Every
subsequent lookup — references, unused, backlinks — is a dictionary hit.

The initial build is **chunked across `EditorApplication.update` ticks behind a cancellable progress bar**, so the editor
stays responsive on a project of this size.

**Cache invalidation.** The cache stores a format version. It is rebuilt automatically when the cache file is missing or
its version differs. While the editor is open, `ReferenceIndexUpdater` keeps it current. Changes made while Unity is
**closed** (a `git pull`, for instance) are not detected — a **`Rebuild Index`** toolbar button covers that case, and is
the documented remedy whenever results look stale.

**Known limitation:** the YAML scan reads what is on disk, so it cannot see **unsaved edits in an open scene**. For
currently-loaded scenes, `OpenSceneReferenceScanner` performs the live component walk (retaining the working
`EditorSceneManager` logic from the existing window) and its results are merged over the index.

### Panes

**Left — discovery.** A UI Toolkit `ListView`. At ~1000 assets, virtualization is mandatory; IMGUI would redraw every
row on every repaint. Provides a search field, a type filter (Events / Int / String / Bool / …), and two health filters
that the index yields for free:

- **Unused** — nothing references the asset. Surfaced as "No references found", never as "safe to delete"; an SO loaded
  from code rather than wired in the inspector would be a false positive.
- **Possible duplicates** — near-identical names (`PlayerDied` / `OnPlayerDied` / `PlayerDeath`). The heuristic is
  deterministic and therefore testable: normalize each name (lowercase, strip a leading `On`, strip non-alphanumeric
  characters), then group names whose normalized forms are within a Levenshtein distance of 2. That groups all three of
  the examples above.

Each row shows name, containing folder, and a **reference count**.

**Right, top — tracing.** References to the selected asset, grouped Scenes / Prefabs / ScriptableObjects. Each row
selects and pings its object.

**Right, bottom — runtime.** Play mode only.

- Reactive variable: current value, plus the existing `Raise` button.
- Event: `Raise Event`, the live subscriber list (via `Delegate.GetInvocationList()`, as the current inspector already
  does), and a timestamped fire log showing frame, time, event name, and subscriber count.

**Watching is opt-in.** A global log cannot be built by subscribing to all 1000 events: `GetEventViewModel()` would
force-load every SO asset and instantiate 1000 view models. The selected asset is watched automatically; further assets
are watched by pinning them.

**Toolbar — authoring.** `+ Create` opens a popup with a type dropdown (populated by reflection over the concrete
non-abstract subclasses of `EventViewModelSO` and `BaseReactiveVariableSO`), a name field, and a target folder. The name
field **warns as the user types** if a similarly-named asset already exists — attacking duplication at the only moment
it is cheap to prevent.

## Prerequisite fix

[ReactiveVariableSO.cs:59](../../../Runtime/Core/InterfaceAdapters/ReactiveVariable/Engine/Core/ReactiveVariableSO.cs#L59)
unsubscribes from the play-mode callback with `+=` instead of `-=`:

```csharp
_isSubscribedToPlayModeChanged = false;
EditorApplication.playModeStateChanged += ChangePlayMode;   // should be -=
```

It re-subscribes rather than unsubscribing *and* clears the guard flag, so the next `GetReactiveVariable()` attaches a
further handler. Handlers multiply on every play-mode cycle. `EventViewModelSO` performs the same teardown correctly
with `-=`.

The runtime pane's live value display depends on this exact callback path, so this is fixed **first**, as its own `[F]`
commit in the package, before anything is built on top of it.

## Data flow

1. Window opens → `MvvmAssetRepository` lists assets → `AssetListPanel` binds them to the `ListView`.
2. `ReferenceIndexBuilder` loads the cached index, or builds it if absent/stale.
3. User selects a row → `ReferencesPanel` queries `ReferenceIndex` by GUID, merges `OpenSceneReferenceScanner` results,
   and renders.
4. Entering play mode → `EventRaiseRecorder` subscribes to watched assets → `RuntimePanel` renders values and the log.
5. Exiting play mode → `EventRaiseRecorder.Dispose()` unsubscribes everything.
6. Any asset import/move/delete → `ReferenceIndexUpdater` patches `ReferenceIndex` for the affected paths only.

## Error handling

- A missing or corrupt index cache triggers a rebuild rather than an exception.
- Unreadable or malformed YAML for a single asset is reported (path + reason) and skipped. **The blanket
  `catch { continue; }` in the existing window is not carried forward** — failures surface, they do not vanish.
- `MvvmAssetFactory` throws when the target folder does not exist or the name collides exactly. Per the package
  standard: throw, do not return null.

## Testing

Unity Test Framework, run from the Editor Test Runner. Tests live in `Tests/Editor` with an asmdef referencing the
runtime + editor asmdefs, `precompiledReferences: ["nunit.framework.dll"]`,
`defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `includePlatforms: ["Editor"]`.

The `AssetDatabase`-facing classes are thin by design so the logic beneath them is testable headlessly:

- `ReferenceIndex` — seeded explicitly with a `guid → paths` map. Lookup, unknown-GUID, and empty-index cases.
- `ReferenceIndexBuilder`'s YAML GUID extraction — fed literal YAML strings, not files.
- The duplicate-name heuristic — fed a list of names, asserts the expected groupings.

Naming `MethodName_WhatConditions_DoesWhat()`, AAA, one assert per test.

## Conventions

Namespace `MVVM.CoreEditor`, in the existing `Editor.MVVM.Core` asmdef. Package code standards apply: one type per file,
no abbreviations, no `var` outside `foreach`, no `else`, no ternaries, no bool parameters, no public fields, guard
clauses, and classes named for the pattern they implement (`Repository`, `Builder`, `Factory`, `Recorder`, `Index`).

Version: minor bump in `package.json` (new feature, no breaking change).
