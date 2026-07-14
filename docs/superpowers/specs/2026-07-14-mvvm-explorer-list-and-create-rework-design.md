# MVVM Explorer — List & Create Popup Rework

Date: 2026-07-14
Package: `UP-MVVM` (`com.mtg.mvvm`)
Branch: `dev/lalo/mvvmExplorerWindow`
Status: Approved, pending implementation plan
Follows: [2026-07-13-mvvm-explorer-window-design.md](2026-07-13-mvvm-explorer-window-design.md)

## Problem

The Explorer shipped and works, but using it surfaced five issues:

1. The asset list looks nothing like `UP-ScreenNavigator`'s list, so the two editor tools feel unrelated.
2. The list is flat. With ~1000 assets a flat list is hard to navigate; it should group.
3. The create popup's type picker is a flat dropdown of 15 reactive-variable types — awkward to use. The kind of asset
   (event vs reactive variable) is implicit, the name is unconstrained, and the target folder must be typed by hand.
4. The search field is visually misaligned with the list.
5. Double-clicking a row does nothing.

### The search-field bug, precisely

`ToolbarSearchField` is a **toolbar** control, but
[AssetListPanel.cs](../../../Editor/Explorer/AssetListPanel.cs) adds it — and an `EnumField` — directly into a bare
`VisualElement`. Toolbar controls outside a `Toolbar` container inherit none of the toolbar's height, background, or
border styling, so they float against the list. This is the whole cause.

## Solution

### 1. List styling — port ScreenNavigator's pattern

Replace `AssetListPanel`'s ad-hoc rows with the structure `UP-ScreenNavigator` already uses, so the two windows read as
one tool:

- `MvvmListPanel : VisualElement` with its own `MvvmExplorerStyles.uss`.
- A `ListView` with a fixed row height, `showAlternatingRowBackgrounds = None`, `makeItem`/`bindItem`.
- Rows are a flat list of `MvvmListRow`, where each row is **either** a collapsible group header **or** an indented
  asset row — mirroring
  [ScreenListRow](../../../../UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenListRow.cs)'s two static factories
  (`CreateGroupHeader` / `CreateAssetRow`).
- Group headers show `▼` / `▶` and toggle on click. Collapsed group names persist across window reloads.

The existing per-row **reference count** is retained — it is the list's most valuable column.

### 2. Grouping — `MvvmGroupNameProvider`

The direct analogue of
[ScreenGroupNameProvider](../../../../UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenGroupNameProvider.cs), which
already solves this exact problem by skipping generic path segments.

**Rule:** read the concrete SO type's `[CreateAssetMenu]` `menuName`, split on `/`, and return the first segment that is
not a generic word. Generic words: `ScriptableObjects`, `ReactiveVariable`, `ReactiveVariables`.

If no segment survives, fall back to the **asset's folder path**, using ScreenNavigator's generic-folder-skipping rule.

**Why the fallback is mandatory, not a nicety.** The menu paths across the codebase have three distinct shapes:

| Shape | Example | First non-generic segment |
| --- | --- | --- |
| `ScriptableObjects/<Feature>/ReactiveVariables/<Type>` | `…/MVVM/ReactiveVariables/Bool` | `MVVM` |
| `ScriptableObjects/ReactiveVariable/<Feature>/<Type>` | `…/ReactiveVariable/Fishing/FishData` | `Fishing` |
| `ScriptableObjects/ReactiveVariable/<Type>` | `…/ReactiveVariable/FriendData` | *(none — folder fallback)* |

And critically: **there is exactly one `EventViewModelSO` type in the entire codebase**
(`ScriptableObjects/MVVM/EventViewModelSO`). Every event asset — likely several hundred — shares it. `CreateAssetMenu`
therefore carries **zero** grouping information for events; without the folder fallback they would all collapse into a
single group named `MVVM`. Events are grouped by folder, always.

The `[CreateAssetMenu]` attribute is read once per **type** via `TypeCache`, not once per asset.

### 3. Create popup

Three stacked steps.

**Kind** — a two-option toggle: `Event` or `Reactive Variable`. Choosing `Event` hides the type picker entirely (one
event type exists, so there is nothing to pick).

**Type** (reactive variables only) — replace the flat 15-item `PopupField` with **`AdvancedDropdown`**
(`UnityEditor.IMGUI.Controls`), the searchable hierarchical control Unity itself uses for the *Add Component* menu. Its
tree is built directly from the `CreateAssetMenu` paths, so it nests exactly like the real Create menu and supports
type-to-filter. No new UI is invented; this is the correct built-in control for the job.

**Name + folder** —
- The user types the meaningful part; the popup appends the concrete type name as a suffix and shows the resulting
  filename live (`PlayerHealth` → `PlayerHealthIntReactiveVariableSO.asset`).
- The folder is a **read-only field plus a Browse button** (`EditorUtility.OpenFolderPanel`), defaulting to the folder
  currently selected in the Project window. A folder outside the project is rejected with a message; it is never
  silently accepted.
- The existing as-you-type duplicate-name warning is retained and now runs against the **final suffixed name**.

### 4. Double-click

`ListView.itemsChosen` → set `Selection.activeObject` and `EditorGUIUtility.PingObject`, selecting and highlighting the
asset in the Project window. Group headers ignore it. Single-click keeps its current behaviour (driving the references
and runtime panes).

### 5. Search field

Wrap the search field and the filter in a `Toolbar` at the top of the list pane. The filter becomes a `ToolbarMenu` so
it matches the search field's styling.

## Non-goals

- **Nested (two-level) group headers.** ScreenNavigator's row model is single-level; a second level would require
  reworking the row/collapse logic for little gain.
- Changing what the references or runtime panes do.
- Re-grouping by anything other than feature (no grouping by type, folder-only, or package).

## Architecture

| Type | Responsibility |
| --- | --- |
| `MvvmListPanel` | Replaces `AssetListPanel`. Owns the `ListView`, the toolbar, grouping, and collapse state. |
| `MvvmListRow` | One row: a group header **or** an asset. Two static factories; `IsGroupHeader()`. |
| `MvvmGroupNameProvider` | `GetGroupNameWithAsset(MvvmAsset)` → feature name from `CreateAssetMenu`, else folder. |
| `CreateAssetMenuReader` | `GetMenuNameWithType(Type)` → the `[CreateAssetMenu]` `menuName`, or empty. `TypeCache`-backed, cached per type. |
| `MvvmTypeDropdown` | `AdvancedDropdown` built from the reactive-variable `CreateAssetMenu` paths. |
| `MvvmAssetNameBuilder` | `GetAssetNameWithTypeAndName(Type, string)` → the final suffixed filename. |
| `CreateMvvmAssetPopup` | Reworked: kind toggle, `MvvmTypeDropdown`, live name preview, folder browse. |
| `MvvmExplorerStyles.uss` | Row, group-header, and toolbar styling. |

`MvvmAssetFactory` keeps its current contract (`Create` throws on a missing folder or an exact name collision).

## Error handling

- A type with no `[CreateAssetMenu]` yields an empty menu name; `MvvmGroupNameProvider` falls back to the folder rather
  than throwing. A type without the attribute is valid — it just cannot be created from the Create menu.
- `EditorUtility.OpenFolderPanel` returns an absolute path and may return a folder outside the project, or an empty
  string on cancel. Both are handled explicitly: cancel leaves the current folder unchanged; an outside-project folder
  is rejected with a message in the existing warning label.
- The duplicate-name check and the empty-name guard both run against the final suffixed name before `Create` is called.

## Testing

Unity Test Framework, EditMode, in the existing `EditMode.Test.MVVM` assembly. Run headlessly via the Unity batch
testbed (scratch project + `-runTests`), not by asking a human to open the Test Runner.

The `AssetDatabase`- and UI-facing classes stay thin so the logic beneath them is testable:

- `MvvmGroupNameProvider` — seeded with menu paths and folder paths as plain strings. Covers all three menu shapes, the
  events case (no menu information → folder), and a path with no non-generic segment at all.
- `MvvmAssetNameBuilder` — a type name plus a typed name produces the expected filename; an empty typed name is
  rejected.
- `CreateAssetMenuReader`'s parsing — fed a menu-name string, not a live `Type`.

Naming `MethodName_WhatConditions_DoesWhat()`, AAA, one assert per test.

## Conventions

Namespace `MVVM.CoreEditor`, in the existing `Editor.MVVM.Core` assembly. Package standards apply: one type per file, no
abbreviations, no `var` outside `foreach`, no `else`, no ternaries, no bool parameters, no public fields, guard clauses,
and classes named for the pattern they implement (`Provider`, `Reader`, `Builder`).

Version: minor bump in `package.json`.
