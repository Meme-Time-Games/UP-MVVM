# MVVM Explorer — List & Create Rework Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restyle the Explorer's asset list to match `UP-ScreenNavigator`, group it by feature (derived from `[CreateAssetMenu]`, falling back to folder), fix the misaligned search field, add double-click-to-ping, and rebuild the create popup around a kind toggle, a searchable hierarchical type picker, an auto-suffixed name, and a folder browser.

**Architecture:** The grouping logic is pulled into two pure, string-in/string-out classes (`CreateAssetMenuReader`, `MvvmGroupNameProvider`) so it is fully unit-testable headlessly; the `ListView` panel is then a thin renderer over them. The row model copies `UP-ScreenNavigator`'s: a flat list where each row is *either* a collapsible group header *or* an indented asset.

**Tech Stack:** Unity 6000.x, C#, UI Toolkit (`ListView`, `Toolbar`, USS), `UnityEditor.IMGUI.Controls.AdvancedDropdown`, `TypeCache`, Unity Test Framework + NUnit (EditMode).

**Spec:** [2026-07-14-mvvm-explorer-list-and-create-rework-design.md](../specs/2026-07-14-mvvm-explorer-list-and-create-rework-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **Repo:** all git runs inside `Packages/UP-MVVM` (the project root is **not** a git repo). Branch `dev/lalo/mvvmExplorerWindow`. Never switch branches.
- **Commit prefixes:** `[A]` add, `[U]` update, `[D]` delete, `[F]` fix.
- **YOU CAN AND MUST RUN THE TESTS.** There is a working Unity batch testbed. Do not ask a human to open the Test Runner.
  - Testbed project: `C:/Users/laloc/AppData/Local/Temp/claude/d--PizzaData-Unity-Projects-MTG-MTG-Packages/479337ac-30b1-47ca-a72e-b6385a12a78d/scratchpad/mvvm-testbed`
  - Run: `"/c/Program Files/Unity/Hub/Editor/6000.2.13f1/Editor/Unity.exe" -batchmode -nographics -projectPath "<testbed>" -runTests -testPlatform EditMode -testResults "<testbed>/results.xml" -logFile "<testbed>/unity.log"`
  - Read the result: `grep -o 'total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' "<testbed>/results.xml"`
  - Read compile errors: `grep -E "error CS[0-9]+" "<testbed>/unity.log" | sort -u`
  - A run takes ~40s. **Baseline before any change in this plan: 33/33 passing.**
  - The testbed references the package **in place**, so your edits are picked up with no copying. It also generates `.meta` files in place — **commit any new `.meta` files Unity creates.**
- **Do NOT use the Unity MCP tools.** They point at a *different* Unity project; a clean console there is a false green.
- **Namespace:** `MVVM.CoreEditor` (tests `MVVM.CoreEditor.Tests`). **Assembly:** existing `Editor.MVVM.Core`; tests in existing `EditMode.Test.MVVM`.
- **Code standards (CLAUDE.md):** one type per file; never abbreviate; self-documenting code, avoid comments; **no `else`** (guard clauses); no ternaries; no `var` outside `foreach`; no public fields; no bool parameters; braces **only** when a body is more than one line, and a single-line `if` body goes on the **next** line; verb-led methods (`Get…` returns a value, `Is…`/`Has…` returns bool, `Set…` assigns), adding `With<Param>` when a parameter is essential; **throw, don't return null**; no Singletons; `ReferenceEquals(x, null)` for plain C# objects but `== null` for `UnityEngine.Object` subclasses (Unity overloads `==` to catch destroyed objects).
- **Test conventions:** `MethodName_WhatConditions_DoesWhat()`; AAA; **exactly one assert per test**.
- **The only allowed `set`** is an event on an interface boundary (e.g. `OnAssetSelected { get; set; }`).

## What already exists (do not re-implement)

- `MvvmAsset`: read-only `string Guid`, `string Path`, `string Name`, `string TypeName`, `bool IsEvent`.
- `MvvmAssetRepository.GetAllAssets() → IReadOnlyList<MvvmAsset>` (sorted by name).
- `ReferenceIndex.GetReferencingPathsWithGuid(string) → IReadOnlyList<string>`; `HasReferencesWithGuid(string) → bool`.
- `DuplicateNameFinder.GetDuplicateGroups(IReadOnlyList<string>) → IReadOnlyList<IReadOnlyList<string>>`; `GetSimilarNamesWithName(string, IReadOnlyList<string>) → IReadOnlyList<string>`.
- `MvvmAssetFactory.GetCreatableTypes() → IReadOnlyList<Type>`; `Create(Type, string assetName, string folderPath) → ScriptableObject` (throws on missing folder / exact collision).
- `AssetListPanel` (`Editor/Explorer/AssetListPanel.cs`) and `AssetListFilter` — **`AssetListPanel` is replaced in Task 6 and deleted; `AssetListFilter` is kept.**
- `MvvmExplorerWindow` (`Editor/Explorer/MvvmExplorerWindow.cs`) — modified in Tasks 4 and 6.
- `CreateMvvmAssetPopup` (`Editor/Authoring/CreateMvvmAssetPopup.cs`) — reworked in Task 8.

## Reference implementation to copy from

`UP-ScreenNavigator` already solves the list-styling and grouping problems. Read these before starting:
- `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenListPanel.cs` — row model, collapse, `itemsChosen`.
- `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenListRow.cs` — the header-or-item row.
- `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenGroupNameProvider.cs` — generic-segment skipping.
- `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapStyles.uss` — theme tokens and row classes.
- `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapWindow.cs` — `ApplyStyleSheet()` and the `theme-dark`/`theme-light` class.

---

### Task 1: CreateAssetMenuReader

Maps a concrete SO **type name** to its `[CreateAssetMenu]` `menuName`. Split into a pure parser (testable) and a `TypeCache` lookup (not testable headlessly without assets, but trivial).

**Files:**
- Create: `Editor/Grouping/CreateAssetMenuReader.cs`
- Test: `Tests/Editor/CreateAssetMenuReaderTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `string GetMenuNameWithTypeName(string typeName)` — the `menuName` of the `[CreateAssetMenu]` on the type with that name; **`string.Empty`** when the type is unknown or carries no attribute. Never null.

  Task 2 and Task 5 consume this.

**Why `string.Empty` and not a throw:** a ScriptableObject type without `[CreateAssetMenu]` is perfectly valid (it just cannot be made from the Create menu). Absence is a normal case here, not a failure, so the Null-Object-ish empty string is correct and the caller falls back to the folder. This is the same reasoning already applied to `MvvmAssetRepository.GetAssetWithGuid`.

- [ ] **Step 1: Write the failing tests**

Create `Tests/Editor/CreateAssetMenuReaderTests.cs`:

```csharp
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class CreateAssetMenuReaderTests
    {
        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsUnknown_ReturnsEmpty()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("NoSuchTypeAnywhereSO");

            Assert.IsEmpty(menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeNameIsNull_ReturnsEmpty()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName(null);

            Assert.IsEmpty(menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsIntReactiveVariable_ReturnsItsMenuName()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("IntReactiveVariableSO");

            Assert.AreEqual("ScriptableObjects/MVVM/ReactiveVariables/Int", menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsEventViewModel_ReturnsItsMenuName()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("EventViewModelSO");

            Assert.AreEqual("ScriptableObjects/MVVM/EventViewModelSO", menuName);
        }
    }
}
```

These two menu names are real — verified against `Runtime/Core/InterfaceAdapters/ReactiveVariable/Engine/Implementations/IntReactiveVariableSO.cs` and `Runtime/Core/InterfaceAdapters/EventViewModel/Engine/EventViewModelSO.cs`.

- [ ] **Step 2: Run the tests, confirm they FAIL**

```bash
TESTBED="C:/Users/laloc/AppData/Local/Temp/claude/d--PizzaData-Unity-Projects-MTG-MTG-Packages/479337ac-30b1-47ca-a72e-b6385a12a78d/scratchpad/mvvm-testbed"
"/c/Program Files/Unity/Hub/Editor/6000.2.13f1/Editor/Unity.exe" -batchmode -nographics -projectPath "$TESTBED" -runTests -testPlatform EditMode -testResults "$TESTBED/results.xml" -logFile "$TESTBED/unity.log"
grep -E "error CS[0-9]+" "$TESTBED/unity.log" | sort -u
```

Expected: compile error — `CreateAssetMenuReader` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Editor/Grouping/CreateAssetMenuReader.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class CreateAssetMenuReader
    {
        private static Dictionary<string, string> _menuNamesByTypeName;

        public string GetMenuNameWithTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return string.Empty;

            Dictionary<string, string> menuNames = GetMenuNames();

            if (!menuNames.ContainsKey(typeName))
                return string.Empty;

            return menuNames[typeName];
        }

        private Dictionary<string, string> GetMenuNames()
        {
            if (!ReferenceEquals(_menuNamesByTypeName, null))
                return _menuNamesByTypeName;

            _menuNamesByTypeName = new Dictionary<string, string>();

            foreach (Type scriptableObjectType in TypeCache.GetTypesDerivedFrom<ScriptableObject>())
                AddMenuNameWithType(scriptableObjectType);

            return _menuNamesByTypeName;
        }

        private void AddMenuNameWithType(Type scriptableObjectType)
        {
            object[] attributes = scriptableObjectType.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false);

            if (attributes.Length == 0)
                return;

            CreateAssetMenuAttribute createAssetMenu = (CreateAssetMenuAttribute)attributes[0];

            if (string.IsNullOrEmpty(createAssetMenu.menuName))
                return;

            _menuNamesByTypeName[scriptableObjectType.Name] = createAssetMenu.menuName;
        }
    }
}
```

The dictionary is `static` so the `TypeCache` sweep runs once per domain, not once per asset. It is a plain static cache, **not** a Singleton — do not introduce a `Singleton<T>`.

- [ ] **Step 4: Run the tests, confirm they PASS**

Same command as Step 2, then:
```bash
grep -o 'total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' "$TESTBED/results.xml"
```
Expected: `total="37" passed="37" failed="0"` (33 baseline + 4 new).

- [ ] **Step 5: Commit** (include any `.meta` Unity generated)

```bash
git -C Packages/UP-MVVM add Editor/Grouping Tests/Editor/CreateAssetMenuReaderTests.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added create asset menu reader"
```

---

### Task 2: MvvmGroupNameProvider

The core of the rework, and pure — menu name and asset path in, group name out. Fully testable.

**Files:**
- Create: `Editor/Grouping/MvvmGroupNameProvider.cs`
- Test: `Tests/Editor/MvvmGroupNameProviderTests.cs`

**Interfaces:**
- Consumes: nothing (takes strings).
- Produces: `string GetGroupNameWithMenuNameAndPath(string menuName, string assetPath)`. Never null; returns `"Ungrouped"` when nothing else resolves. Task 5 consumes this.

**The rule (implement exactly):**
1. Split `menuName` on `/`. Return the first segment that is **not** generic. Generic menu segments: `scriptableobjects`, `reactivevariable`, `reactivevariables` (compared case-insensitively).
2. If that yields nothing — which is the case for **every event** (all events share the single type `EventViewModelSO`, whose menu is `ScriptableObjects/MVVM/EventViewModelSO`… see the note below) and for the flat `ScriptableObjects/ReactiveVariable/<Type>` packages — fall back to the **asset path**: split on `/`, walk from the second-to-last segment backwards, and return the first folder that is not generic. Generic folder names: `assets`, `packages`, `data`, `scriptableobject`, `scriptableobjects`, `so`, `sos`, `resources`, `prefabs`, `scene`, `scenes`, `settings`, `config`, `configs`, `runtime`, `editor`, `events`, `event`, `reactivevariable`, `reactivevariables`, `variables`.
3. If the folder fallback also yields nothing, return `"Ungrouped"`.

**Two subtleties that the rule above already encodes. Both matter; do not "simplify" them away.**

**(a) The last menu segment is a type leaf, never a feature.** In `ScriptableObjects/ReactiveVariable/FriendData`, the only non-generic segment is the final one, `FriendData` — which is a *type*, not a feature. Grouping by it would produce a one-asset group per type. So the menu scan considers **only segments before the last** (the loop runs to `segments.Length - 1`, exclusive). With no candidate left, that path correctly falls through to the folder.

**(b) Events never consult the menu at all — the caller enforces this.** Every event asset in the project shares the single type `EventViewModelSO`, whose menu is `ScriptableObjects/MVVM/EventViewModelSO`. Its first non-generic segment before the leaf is `MVVM`, so a literal menu lookup would put **100% of events into one group called `MVVM`** — precisely the useless outcome the spec rejects.

The fix is at the **call site**, not in this class: Task 5 passes `string.Empty` as the `menuName` whenever `MvvmAsset.IsEvent` is true, which forces the folder fallback. `MvvmGroupNameProvider` stays pure and dumb — empty menu name means "use the folder" — and the single policy decision lives in one obvious place. Task 5's `GetMenuNameWithAsset` does exactly this.

- [ ] **Step 1: Write the failing tests**

Create `Tests/Editor/MvvmGroupNameProviderTests.cs`:

```csharp
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmGroupNameProviderTests
    {
        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasFeatureBeforeReactiveVariables_ReturnsTheFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/MVVM/ReactiveVariables/Bool", "Assets/Whatever/A.asset");

            Assert.AreEqual("MVVM", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasFeatureAfterReactiveVariable_ReturnsTheFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/ReactiveVariable/Fishing/FishData", "Assets/Whatever/A.asset");

            Assert.AreEqual("Fishing", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasNoFeatureSegment_FallsBackToTheFolder()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/ReactiveVariable/FriendData", "Assets/Friends/Data/FriendData.asset");

            Assert.AreEqual("Friends", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuNameIsEmpty_FallsBackToTheFolder()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                string.Empty, "Assets/Chat/Events/OnMessageReceived.asset");

            Assert.AreEqual("Chat", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenFolderIsAllGenericNames_ReturnsUngrouped()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                string.Empty, "Assets/Resources/OnThing.asset");

            Assert.AreEqual("Ungrouped", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuIsPokerNestedUnderMinigames_ReturnsTheFirstFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/Minigames/Poker/ReactiveVariables/PokerRoomData", "Assets/A.asset");

            Assert.AreEqual("Minigames", groupName);
        }
    }
}
```

The last test pins a real consequence the spec accepted: Poker's menu nests under `Minigames`, so its group is `Minigames`, not `Poker`. That is intended, not a bug.

- [ ] **Step 2: Run the tests, confirm they FAIL**

Expected: compile error — `MvvmGroupNameProvider` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Editor/Grouping/MvvmGroupNameProvider.cs`:

```csharp
using System;

namespace MVVM.CoreEditor
{
    public class MvvmGroupNameProvider
    {
        private const string UngroupedName = "Ungrouped";

        private static readonly string[] GenericMenuSegments =
        {
            "scriptableobjects",
            "reactivevariable",
            "reactivevariables"
        };

        private static readonly string[] GenericFolderNames =
        {
            "assets",
            "packages",
            "data",
            "scriptableobject",
            "scriptableobjects",
            "so",
            "sos",
            "resources",
            "prefabs",
            "scene",
            "scenes",
            "settings",
            "config",
            "configs",
            "runtime",
            "editor",
            "event",
            "events",
            "reactivevariable",
            "reactivevariables",
            "variables"
        };

        public string GetGroupNameWithMenuNameAndPath(string menuName, string assetPath)
        {
            string featureFromMenu = GetFeatureFromMenuName(menuName);

            if (!string.IsNullOrEmpty(featureFromMenu))
                return featureFromMenu;

            return GetFeatureFromAssetPath(assetPath);
        }

        private string GetFeatureFromMenuName(string menuName)
        {
            if (string.IsNullOrEmpty(menuName))
                return string.Empty;

            string[] segments = menuName.Split('/');

            for (int segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
            {
                if (IsGenericName(segments[segmentIndex], GenericMenuSegments))
                    continue;

                return segments[segmentIndex];
            }

            return string.Empty;
        }

        private string GetFeatureFromAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return UngroupedName;

            string[] segments = assetPath.Split('/');

            for (int segmentIndex = segments.Length - 2; segmentIndex >= 0; segmentIndex--)
            {
                if (IsGenericName(segments[segmentIndex], GenericFolderNames))
                    continue;

                return segments[segmentIndex];
            }

            return UngroupedName;
        }

        private bool IsGenericName(string name, string[] genericNames)
        {
            string normalizedName = name.ToLowerInvariant();

            foreach (string genericName in genericNames)
            {
                if (normalizedName == genericName)
                    return true;
            }

            return false;
        }
    }
}
```

Note `GetFeatureFromMenuName` iterates to `segments.Length - 1` (exclusive), so the **last** segment — the type leaf — is never a candidate. That is what makes `ScriptableObjects/ReactiveVariable/FriendData` fall through to the folder instead of grouping under `FriendData`.

The generic-folder list duplicates `UP-ScreenNavigator`'s `ScreenGroupNameProvider`. That is deliberate: referencing another package just to share a `string[]` would add a real package dependency for no benefit. The lists are allowed to diverge (this one adds `event`/`events`/`variables`).

- [ ] **Step 4: Run the tests, confirm they PASS**

Expected: `total="43" passed="43"` (37 + 6 new).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Grouping/MvvmGroupNameProvider.cs Tests/Editor/MvvmGroupNameProviderTests.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added mvvm group name provider"
```

---

### Task 3: MvvmAssetNameBuilder

Builds the final asset filename by suffixing the typed name with the concrete type name.

**Files:**
- Create: `Editor/Authoring/MvvmAssetNameBuilder.cs`
- Test: `Tests/Editor/MvvmAssetNameBuilderTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `string GetAssetNameWithTypeNameAndName(string typeName, string name)` — **throws `ArgumentException`** when `name` is null/empty/whitespace. Task 8 consumes this.

Throwing on an empty name is correct here (unlike the empty-menu case): an empty name is a *failure*, not a valid absence. Task 8 guards before calling, and surfaces the message in the popup.

- [ ] **Step 1: Write the failing tests**

Create `Tests/Editor/MvvmAssetNameBuilderTests.cs`:

```csharp
using System;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmAssetNameBuilderTests
    {
        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsGiven_AppendsTheTypeName()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", "PlayerHealth");

            Assert.AreEqual("PlayerHealthIntReactiveVariableSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameAlreadyEndsWithTheTypeName_DoesNotAppendItTwice()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName(
                "IntReactiveVariableSO", "PlayerHealthIntReactiveVariableSO");

            Assert.AreEqual("PlayerHealthIntReactiveVariableSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameHasSurroundingWhitespace_TrimsIt()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName("EventViewModelSO", "  OnPlayerDied  ");

            Assert.AreEqual("OnPlayerDiedEventViewModelSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsWhitespace_ThrowsArgumentException()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            Assert.Throws<ArgumentException>(
                () => builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", "   "));
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsNull_ThrowsArgumentException()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            Assert.Throws<ArgumentException>(
                () => builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", null));
        }
    }
}
```

- [ ] **Step 2: Run the tests, confirm they FAIL**

Expected: compile error — `MvvmAssetNameBuilder` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Editor/Authoring/MvvmAssetNameBuilder.cs`:

```csharp
using System;

namespace MVVM.CoreEditor
{
    public class MvvmAssetNameBuilder
    {
        public string GetAssetNameWithTypeNameAndName(string typeName, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The asset name cannot be empty.", nameof(name));

            string trimmedName = name.Trim();

            if (trimmedName.EndsWith(typeName, StringComparison.Ordinal))
                return trimmedName;

            return trimmedName + typeName;
        }
    }
}
```

- [ ] **Step 4: Run the tests, confirm they PASS**

Expected: `total="48" passed="48"` (43 + 5 new).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Authoring/MvvmAssetNameBuilder.cs Tests/Editor/MvvmAssetNameBuilderTests.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added mvvm asset name builder"
```

---

### Task 4: Stylesheet and theme wiring

Adds the USS and makes the window load it, so Task 5's classes have something to style.

**Files:**
- Create: `Editor/Explorer/MvvmExplorerStyles.uss`
- Modify: `Editor/Explorer/MvvmExplorerWindow.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: the window root carries class `mvvm-explorer` plus `theme-dark` or `theme-light`, and has `MvvmExplorerStyles.uss` applied. Task 5's classes depend on this.

- [ ] **Step 1: Write the stylesheet**

Create `Editor/Explorer/MvvmExplorerStyles.uss`:

```css
.theme-dark {
    --mv-panel: rgb(56, 56, 56);
    --mv-card-soft: rgb(63, 63, 63);
    --mv-border: rgb(35, 35, 35);
    --mv-text: rgb(210, 210, 210);
    --mv-text-strong: rgb(240, 240, 240);
    --mv-muted: rgb(138, 138, 138);
    --mv-hover: rgb(74, 74, 74);
    --mv-selected: rgb(44, 93, 135);
}

.theme-light {
    --mv-panel: rgb(200, 200, 200);
    --mv-card-soft: rgb(208, 208, 208);
    --mv-border: rgb(164, 164, 164);
    --mv-text: rgb(28, 28, 28);
    --mv-text-strong: rgb(12, 12, 12);
    --mv-muted: rgb(96, 96, 96);
    --mv-hover: rgb(190, 190, 190);
    --mv-selected: rgb(58, 114, 176);
}

.mvvm-explorer {
    flex-grow: 1;
    color: var(--mv-text);
}

.mvvm-list-panel {
    flex-grow: 1;
    background-color: var(--mv-panel);
    border-right-width: 1px;
    border-right-color: var(--mv-border);
}

.mvvm-search-row {
    flex-direction: row;
    align-items: center;
    padding-left: 4px;
    padding-right: 4px;
    padding-top: 3px;
    padding-bottom: 3px;
    background-color: var(--mv-panel);
    border-bottom-width: 1px;
    border-bottom-color: var(--mv-border);
}

.mvvm-search {
    flex-grow: 1;
    margin-left: 0;
    margin-right: 4px;
}

.mvvm-filter {
    flex-shrink: 0;
    width: 130px;
    margin: 0;
}

.mvvm-list {
    flex-grow: 1;
}

.mvvm-list .unity-collection-view__item:hover {
    background-color: var(--mv-hover);
}

.mvvm-list .unity-collection-view__item--selected {
    background-color: var(--mv-selected);
}

.list-row {
    flex-grow: 1;
    height: 100%;
}

.group-header {
    height: 100%;
    padding-left: 6px;
    padding-right: 6px;
    font-size: 9px;
    -unity-font-style: bold;
    letter-spacing: 1px;
    color: var(--mv-muted);
    -unity-text-align: middle-left;
    background-color: var(--mv-card-soft);
    border-bottom-width: 1px;
    border-bottom-color: var(--mv-border);
}

.group-header:hover {
    background-color: var(--mv-hover);
    color: var(--mv-text-strong);
}

.asset-row {
    flex-direction: row;
    align-items: center;
    height: 100%;
    padding-left: 8px;
    padding-right: 6px;
}

.asset-row__name {
    flex-grow: 1;
    flex-shrink: 1;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    -unity-text-align: middle-left;
    font-size: 12px;
    color: var(--mv-text);
}

.asset-row__count {
    flex-shrink: 0;
    margin-left: 6px;
    font-size: 10px;
    color: var(--mv-muted);
    -unity-text-align: middle-right;
}

.is-hidden {
    display: none;
}
```

- [ ] **Step 2: Wire the stylesheet and theme into the window**

In `Editor/Explorer/MvvmExplorerWindow.cs`, add the constant at the top of the class:

```csharp
        private const string StyleSheetPath =
            "Packages/com.mtg.mvvm/Editor/Explorer/MvvmExplorerStyles.uss";
```

At the **start** of `CreateGUI()` (before any element is added), add:

```csharp
            rootVisualElement.AddToClassList("mvvm-explorer");
            ApplyTheme();
            ApplyStyleSheet();
```

And add these two methods:

```csharp
        private void ApplyTheme()
        {
            if (EditorGUIUtility.isProSkin)
            {
                rootVisualElement.AddToClassList("theme-dark");
                return;
            }

            rootVisualElement.AddToClassList("theme-light");
        }

        private void ApplyStyleSheet()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);

            if (styleSheet == null)
                return;

            rootVisualElement.styleSheets.Add(styleSheet);
        }
```

`StyleSheet` needs `using UnityEngine.UIElements;` (already present); `EditorGUIUtility` and `AssetDatabase` need `using UnityEditor;` (already present).

The `styleSheet == null` guard uses `==`, not `ReferenceEquals` — `StyleSheet` is a `UnityEngine.Object`, so Unity's destroyed-object overload must apply.

`com.mtg.mvvm` is the package name from `package.json` — the path is how Unity addresses files inside a package.

- [ ] **Step 3: Run the tests, confirm still green**

Expected: `total="48" passed="48"` — no new tests; this task just must not break the build. Check for compile errors.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer/MvvmExplorerStyles.uss Editor/Explorer/MvvmExplorerWindow.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added mvvm explorer stylesheet and theme wiring"
```

---

### Task 5: MvvmListRow + MvvmListPanel

Replaces `AssetListPanel` with the grouped, ScreenNavigator-styled list. This is the biggest task.

**Files:**
- Create: `Editor/Explorer/MvvmListRow.cs`
- Create: `Editor/Explorer/MvvmListPanel.cs`
- Test: `Tests/Editor/MvvmListRowTests.cs`

**Interfaces:**
- Consumes: `MvvmAsset`; `ReferenceIndex`; `DuplicateNameFinder`; `AssetListFilter`; `CreateAssetMenuReader` (Task 1); `MvvmGroupNameProvider` (Task 2).
- Produces:
  - `MvvmListRow.CreateGroupHeader(string groupName) → MvvmListRow`
  - `MvvmListRow.CreateAssetRow(MvvmAsset asset) → MvvmListRow`
  - `MvvmListRow.Asset` / `.GroupName` / `bool IsGroupHeader()`
  - `MvvmListPanel.Root` (`VisualElement`); `SetAssets(IReadOnlyList<MvvmAsset>, ReferenceIndex)`; `SelectAssetWithGuid(string)`; `Action<MvvmAsset> OnAssetSelected { get; set; }`

  Task 6 consumes the `MvvmListPanel` members.

- [ ] **Step 1: Write MvvmListRow and its failing tests**

Create `Tests/Editor/MvvmListRowTests.cs`:

```csharp
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmListRowTests
    {
        [Test]
        public void IsGroupHeader_WhenRowIsAGroupHeader_ReturnsTrue()
        {
            MvvmListRow row = MvvmListRow.CreateGroupHeader("MVVM");

            Assert.IsTrue(row.IsGroupHeader());
        }

        [Test]
        public void IsGroupHeader_WhenRowIsAnAsset_ReturnsFalse()
        {
            MvvmAsset asset = new MvvmAsset("guid", "Assets/A.asset", "A", "IntReactiveVariableSO", false);

            MvvmListRow row = MvvmListRow.CreateAssetRow(asset);

            Assert.IsFalse(row.IsGroupHeader());
        }

        [Test]
        public void Asset_WhenRowIsAnAsset_ReturnsTheAsset()
        {
            MvvmAsset asset = new MvvmAsset("guid", "Assets/A.asset", "A", "IntReactiveVariableSO", false);

            MvvmListRow row = MvvmListRow.CreateAssetRow(asset);

            Assert.AreSame(asset, row.Asset);
        }

        [Test]
        public void GroupName_WhenRowIsAGroupHeader_ReturnsTheGroupName()
        {
            MvvmListRow row = MvvmListRow.CreateGroupHeader("Fishing");

            Assert.AreEqual("Fishing", row.GroupName);
        }
    }
}
```

Create `Editor/Explorer/MvvmListRow.cs`:

```csharp
namespace MVVM.CoreEditor
{
    public class MvvmListRow
    {
        private readonly MvvmAsset _asset;
        private readonly string _groupName;

        public MvvmAsset Asset => _asset;
        public string GroupName => _groupName;

        private MvvmListRow(MvvmAsset asset, string groupName)
        {
            _asset = asset;
            _groupName = groupName;
        }

        public static MvvmListRow CreateAssetRow(MvvmAsset asset)
        {
            return new MvvmListRow(asset, null);
        }

        public static MvvmListRow CreateGroupHeader(string groupName)
        {
            return new MvvmListRow(null, groupName);
        }

        public bool IsGroupHeader()
        {
            return !string.IsNullOrEmpty(_groupName);
        }
    }
}
```

- [ ] **Step 2: Write MvvmListPanel**

Create `Editor/Explorer/MvvmListPanel.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class MvvmListPanel
    {
        private const int RowHeight = 22;

        private readonly VisualElement _root = new VisualElement();
        private readonly ListView _listView = new ListView();
        private readonly ToolbarSearchField _searchField = new ToolbarSearchField();
        private readonly EnumField _filterField = new EnumField(AssetListFilter.All);
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();
        private readonly CreateAssetMenuReader _createAssetMenuReader = new CreateAssetMenuReader();
        private readonly MvvmGroupNameProvider _groupNameProvider = new MvvmGroupNameProvider();

        private readonly List<MvvmListRow> _visibleRows = new List<MvvmListRow>();
        private readonly HashSet<string> _collapsedGroups = new HashSet<string>();

        private IReadOnlyList<MvvmAsset> _allAssets = Array.Empty<MvvmAsset>();
        private ReferenceIndex _referenceIndex;
        private HashSet<string> _duplicateNames = new HashSet<string>();

        public VisualElement Root => _root;
        public Action<MvvmAsset> OnAssetSelected { get; set; }

        public MvvmListPanel()
        {
            _root.AddToClassList("mvvm-list-panel");

            _searchField.AddToClassList("mvvm-search");
            _searchField.RegisterValueChangedCallback(searchChange => RebuildRows());

            _filterField.AddToClassList("mvvm-filter");
            _filterField.RegisterValueChangedCallback(filterChange => RebuildRows());

            VisualElement searchRow = new VisualElement();
            searchRow.AddToClassList("mvvm-search-row");
            searchRow.Add(_searchField);
            searchRow.Add(_filterField);

            _listView.AddToClassList("mvvm-list");
            _listView.fixedItemHeight = RowHeight;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            _listView.selectionType = SelectionType.Single;
            _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            _listView.itemsSource = _visibleRows;
            _listView.makeItem = CreateRow;
            _listView.bindItem = BindRow;
            _listView.selectionChanged += RaiseAssetSelected;
            _listView.itemsChosen += PingChosenAsset;

            _root.Add(searchRow);
            _root.Add(_listView);
        }

        public void SetAssets(IReadOnlyList<MvvmAsset> assets, ReferenceIndex referenceIndex)
        {
            _allAssets = assets;
            _referenceIndex = referenceIndex;
            _duplicateNames = GetDuplicateNames(assets);

            RebuildRows();
        }

        public void SelectAssetWithGuid(string guid)
        {
            int rowIndex = _visibleRows.FindIndex(row => IsRowForGuid(row, guid));

            if (rowIndex < 0)
                return;

            _listView.SetSelection(rowIndex);
            _listView.ScrollToItem(rowIndex);
        }

        private bool IsRowForGuid(MvvmListRow row, string guid)
        {
            if (row.IsGroupHeader())
                return false;

            return row.Asset.Guid == guid;
        }

        private void RebuildRows()
        {
            _visibleRows.Clear();

            Dictionary<string, List<MvvmAsset>> groups = GetGroups();
            List<string> groupNames = groups.Keys.ToList();
            groupNames.Sort(CompareGroupNames);

            foreach (string groupName in groupNames)
                AddGroupRows(groupName, groups[groupName]);

            _listView.RefreshItems();
        }

        private void AddGroupRows(string groupName, List<MvvmAsset> assets)
        {
            _visibleRows.Add(MvvmListRow.CreateGroupHeader(groupName));

            if (_collapsedGroups.Contains(groupName))
                return;

            assets.Sort(CompareAssetNames);

            foreach (MvvmAsset asset in assets)
                _visibleRows.Add(MvvmListRow.CreateAssetRow(asset));
        }

        private Dictionary<string, List<MvvmAsset>> GetGroups()
        {
            Dictionary<string, List<MvvmAsset>> groups = new Dictionary<string, List<MvvmAsset>>();

            foreach (MvvmAsset asset in _allAssets)
            {
                if (!IsAssetVisible(asset))
                    continue;

                string groupName = GetGroupNameWithAsset(asset);

                if (!groups.ContainsKey(groupName))
                    groups.Add(groupName, new List<MvvmAsset>());

                groups[groupName].Add(asset);
            }

            return groups;
        }

        private string GetGroupNameWithAsset(MvvmAsset asset)
        {
            string menuName = GetMenuNameWithAsset(asset);

            return _groupNameProvider.GetGroupNameWithMenuNameAndPath(menuName, asset.Path);
        }

        private string GetMenuNameWithAsset(MvvmAsset asset)
        {
            if (asset.IsEvent)
                return string.Empty;

            return _createAssetMenuReader.GetMenuNameWithTypeName(asset.TypeName);
        }

        private HashSet<string> GetDuplicateNames(IReadOnlyList<MvvmAsset> assets)
        {
            List<string> names = assets.Select(asset => asset.Name).ToList();
            HashSet<string> duplicateNames = new HashSet<string>();

            foreach (IReadOnlyList<string> duplicateGroup in _duplicateNameFinder.GetDuplicateGroups(names))
            {
                foreach (string duplicateName in duplicateGroup)
                    duplicateNames.Add(duplicateName);
            }

            return duplicateNames;
        }

        private bool IsAssetVisible(MvvmAsset asset)
        {
            if (!HasSearchMatch(asset))
                return false;

            return HasFilterMatch(asset);
        }

        private bool HasSearchMatch(MvvmAsset asset)
        {
            if (string.IsNullOrEmpty(_searchField.value))
                return true;

            return asset.Name.IndexOf(_searchField.value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasFilterMatch(MvvmAsset asset)
        {
            AssetListFilter filter = (AssetListFilter)_filterField.value;

            if (filter == AssetListFilter.All)
                return true;

            if (filter == AssetListFilter.Events)
                return asset.IsEvent;

            if (filter == AssetListFilter.ReactiveVariables)
                return !asset.IsEvent;

            if (filter == AssetListFilter.Unused)
                return IsAssetUnused(asset);

            return _duplicateNames.Contains(asset.Name);
        }

        private bool IsAssetUnused(MvvmAsset asset)
        {
            if (ReferenceEquals(_referenceIndex, null))
                return false;

            return !_referenceIndex.HasReferencesWithGuid(asset.Guid);
        }

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("list-row");

            Label groupHeader = new Label();
            groupHeader.name = "group-header";
            groupHeader.AddToClassList("group-header");
            groupHeader.RegisterCallback<ClickEvent>(ToggleGroupFromHeader);
            row.Add(groupHeader);

            VisualElement content = new VisualElement();
            content.name = "asset-content";
            content.AddToClassList("asset-row");

            Label nameLabel = new Label();
            nameLabel.name = "name";
            nameLabel.AddToClassList("asset-row__name");
            content.Add(nameLabel);

            Label countLabel = new Label();
            countLabel.name = "count";
            countLabel.AddToClassList("asset-row__count");
            content.Add(countLabel);

            row.Add(content);

            return row;
        }

        private void BindRow(VisualElement row, int rowIndex)
        {
            MvvmListRow listRow = _visibleRows[rowIndex];

            if (listRow.IsGroupHeader())
            {
                BindGroupHeader(row, listRow);
                return;
            }

            BindAssetRow(row, listRow);
        }

        private void BindGroupHeader(VisualElement row, MvvmListRow listRow)
        {
            Label groupHeader = row.Q<Label>("group-header");
            groupHeader.userData = listRow.GroupName;
            groupHeader.text = GetFoldoutArrow(listRow.GroupName) + "  " + listRow.GroupName;
            groupHeader.tooltip = "Click to collapse or expand";
            groupHeader.RemoveFromClassList("is-hidden");

            row.Q("asset-content").AddToClassList("is-hidden");
        }

        private void BindAssetRow(VisualElement row, MvvmListRow listRow)
        {
            row.Q<Label>("group-header").AddToClassList("is-hidden");

            VisualElement content = row.Q("asset-content");
            content.RemoveFromClassList("is-hidden");

            MvvmAsset asset = listRow.Asset;

            Label nameLabel = content.Q<Label>("name");
            nameLabel.text = asset.Name;
            nameLabel.tooltip = asset.Path;

            content.Q<Label>("count").text = GetReferenceCountLabel(asset);
        }

        private string GetFoldoutArrow(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
                return "▶";

            return "▼";
        }

        private string GetReferenceCountLabel(MvvmAsset asset)
        {
            if (ReferenceEquals(_referenceIndex, null))
                return string.Empty;

            return _referenceIndex.GetReferencingPathsWithGuid(asset.Guid).Count.ToString();
        }

        private void ToggleGroupFromHeader(ClickEvent clickEvent)
        {
            Label groupHeader = clickEvent.currentTarget as Label;

            if (ReferenceEquals(groupHeader, null))
                return;

            string groupName = groupHeader.userData as string;

            if (string.IsNullOrEmpty(groupName))
                return;

            ToggleGroup(groupName);
        }

        private void ToggleGroup(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
            {
                _collapsedGroups.Remove(groupName);
                RebuildRows();
                return;
            }

            _collapsedGroups.Add(groupName);
            RebuildRows();
        }

        private void RaiseAssetSelected(IEnumerable<object> selectedItems)
        {
            MvvmListRow selectedRow = selectedItems.FirstOrDefault() as MvvmListRow;

            if (ReferenceEquals(selectedRow, null))
                return;

            if (selectedRow.IsGroupHeader())
                return;

            OnAssetSelected?.Invoke(selectedRow.Asset);
        }

        private void PingChosenAsset(IEnumerable<object> chosenItems)
        {
            MvvmListRow chosenRow = chosenItems.FirstOrDefault() as MvvmListRow;

            if (ReferenceEquals(chosenRow, null))
                return;

            if (chosenRow.IsGroupHeader())
                return;

            Object asset = AssetDatabase.LoadMainAssetAtPath(chosenRow.Asset.Path);

            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private int CompareGroupNames(string first, string second)
        {
            return string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareAssetNames(MvvmAsset first, MvvmAsset second)
        {
            return string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
```

`Object` here is `UnityEngine.Object` — add `using Object = UnityEngine.Object;` to disambiguate from `System.Object`.

`itemsChosen` is UI Toolkit's double-click/Enter event — that is requirement 4 (double-click pings the asset in the Project window).

**The async-safety invariant from the previous branch still holds and must not be broken:** `_allAssets` starts as `Array.Empty<MvvmAsset>()` and `_referenceIndex` is only ever assigned together with it inside `SetAssets`. `RebuildRows()` iterates `_allAssets`, so before the index arrives it iterates zero assets and never dereferences `_referenceIndex`. `GetReferenceCountLabel` and `IsAssetUnused` also guard with `ReferenceEquals(_referenceIndex, null)`. Keep all of that.

- [ ] **Step 3: Run the tests, confirm they PASS**

Expected: `total="52" passed="52"` (48 + 4 new). Also check for compile errors — this is the largest new file.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer/MvvmListRow.cs Editor/Explorer/MvvmListPanel.cs Tests/Editor/MvvmListRowTests.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added grouped mvvm list panel and row"
```

---

### Task 6: Swap the window to MvvmListPanel and delete AssetListPanel

**Files:**
- Modify: `Editor/Explorer/MvvmExplorerWindow.cs`
- Delete: `Editor/Explorer/AssetListPanel.cs` (+ `.meta`)

**Interfaces:**
- Consumes: `MvvmListPanel` (Task 5).
- Produces: nothing new. `MvvmExplorerWindow.ShowWindow()` / `ShowWindowWithAsset(Object)` keep their signatures — `EventViewModelEditor` and `ReactiveVariableEditor` call them.

- [ ] **Step 1: Replace the field and its construction**

In `MvvmExplorerWindow.cs`, change the field:

```csharp
        private MvvmListPanel _assetListPanel;
```

and its construction in `CreateGUI`:

```csharp
            _assetListPanel = new MvvmListPanel();
```

Everything else about the window is unchanged: `_assetListPanel.OnAssetSelected = ShowAssetDetail;`, `splitView.Add(_assetListPanel.Root);`, `_assetListPanel.SetAssets(...)` and `_assetListPanel.SelectAssetWithGuid(...)` all keep working, because `MvvmListPanel` exposes the same four members `AssetListPanel` did.

**Read the current file before editing** — it has an `OnDisable` that disposes `_runtimePanel`, a `_pendingSelectionGuid`, and the Task 4 theme/stylesheet calls. Do not disturb them.

- [ ] **Step 2: Delete the old panel**

```bash
git -C Packages/UP-MVVM rm Editor/Explorer/AssetListPanel.cs Editor/Explorer/AssetListPanel.cs.meta
```

`AssetListFilter.cs` **stays** — `MvvmListPanel` uses it.

- [ ] **Step 3: Confirm nothing still references AssetListPanel**

```bash
grep -rn "AssetListPanel" Packages/UP-MVVM --include=*.cs
```
Expected: no matches (the field is now typed `MvvmListPanel`). If anything matches, fix it before running tests.

- [ ] **Step 4: Run the tests, confirm they PASS**

Expected: `total="52" passed="52"`, zero compile errors.

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer/MvvmExplorerWindow.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[U] Updated explorer window to the grouped list panel"
```

---

### Task 7: MvvmTypeDropdown

The searchable, hierarchical type picker — `AdvancedDropdown` is the control Unity itself uses for *Add Component*.

**Files:**
- Create: `Editor/Authoring/MvvmTypeDropdown.cs`
- Create: `Editor/Authoring/MvvmTypeDropdownItem.cs`

**Interfaces:**
- Consumes: `CreateAssetMenuReader` (Task 1).
- Produces:
  - `MvvmTypeDropdown(AdvancedDropdownState state, IReadOnlyList<Type> types, Action<Type> onTypeSelected)`
  - Inherits `AdvancedDropdown.Show(Rect rect)`.

  Task 8 consumes this.

**No tests.** `AdvancedDropdown` is an IMGUI control that cannot be exercised headlessly. The logic worth testing (menu-name parsing) already lives in `CreateAssetMenuReader`, which Task 1 tested.

- [ ] **Step 1: Write the dropdown item**

Create `Editor/Authoring/MvvmTypeDropdownItem.cs`:

```csharp
using System;
using UnityEditor.IMGUI.Controls;

namespace MVVM.CoreEditor
{
    public class MvvmTypeDropdownItem : AdvancedDropdownItem
    {
        private readonly Type _type;

        public Type Type => _type;

        public MvvmTypeDropdownItem(string name, Type type) : base(name)
        {
            _type = type;
        }
    }
}
```

- [ ] **Step 2: Write the dropdown**

Create `Editor/Authoring/MvvmTypeDropdown.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace MVVM.CoreEditor
{
    public class MvvmTypeDropdown : AdvancedDropdown
    {
        private const string RootName = "Reactive Variable";
        private const string MenuPrefix = "ScriptableObjects/";

        private readonly CreateAssetMenuReader _createAssetMenuReader = new CreateAssetMenuReader();
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _onTypeSelected;

        public MvvmTypeDropdown(
            AdvancedDropdownState state, IReadOnlyList<Type> types, Action<Type> onTypeSelected) : base(state)
        {
            _types = types;
            _onTypeSelected = onTypeSelected;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem(RootName);

            foreach (Type type in _types)
                AddTypeItem(root, type);

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            MvvmTypeDropdownItem typeItem = item as MvvmTypeDropdownItem;

            if (ReferenceEquals(typeItem, null))
                return;

            _onTypeSelected?.Invoke(typeItem.Type);
        }

        private void AddTypeItem(AdvancedDropdownItem root, Type type)
        {
            string[] segments = GetMenuSegmentsWithType(type);
            AdvancedDropdownItem parent = root;

            for (int segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
                parent = GetOrCreateChild(parent, segments[segmentIndex]);

            parent.AddChild(new MvvmTypeDropdownItem(segments[segments.Length - 1], type));
        }

        private string[] GetMenuSegmentsWithType(Type type)
        {
            string menuName = _createAssetMenuReader.GetMenuNameWithTypeName(type.Name);

            if (string.IsNullOrEmpty(menuName))
                return new[] { type.Name };

            return GetMenuNameWithoutPrefix(menuName).Split('/');
        }

        private string GetMenuNameWithoutPrefix(string menuName)
        {
            if (!menuName.StartsWith(MenuPrefix, StringComparison.Ordinal))
                return menuName;

            return menuName.Substring(MenuPrefix.Length);
        }

        private AdvancedDropdownItem GetOrCreateChild(AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem child in parent.children)
            {
                if (child.name == name)
                    return child;
            }

            AdvancedDropdownItem createdChild = new AdvancedDropdownItem(name);
            parent.AddChild(createdChild);

            return createdChild;
        }
    }
}
```

A type with no `[CreateAssetMenu]` falls back to a flat entry named after the type — it still appears rather than vanishing.

- [ ] **Step 3: Run the tests, confirm still green**

Expected: `total="52" passed="52"`, zero compile errors. `AdvancedDropdown` / `AdvancedDropdownItem` / `AdvancedDropdownState` are in `UnityEditor.IMGUI.Controls` — if they fail to resolve, that is a real finding; report it, do not silently swap to a `PopupField`.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Authoring/MvvmTypeDropdown.cs Editor/Authoring/MvvmTypeDropdownItem.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[A] Added searchable mvvm type dropdown"
```

---

### Task 8: Rework CreateMvvmAssetPopup

**Files:**
- Modify: `Editor/Authoring/CreateMvvmAssetPopup.cs` (substantial rewrite)

**Interfaces:**
- Consumes: `MvvmAssetFactory` (`GetCreatableTypes()`, `Create(Type, string, string)`); `MvvmAssetRepository`; `DuplicateNameFinder.GetSimilarNamesWithName`; `MvvmTypeDropdown` (Task 7); `MvvmAssetNameBuilder` (Task 3); `MvvmExplorerWindow.ShowPopup`-style entry (keep the existing `Action onCreated` callback contract).
- Produces: nothing new.

**Read the current file first.** It already has: a cached `_existingAssetNames` list built once in `CreateGUI` (keep this — it exists because rebuilding it per keystroke was a Critical perf bug), specific catches around `Create`, an empty-name guard, and an `OnCreated` callback. Preserve all of that behaviour; only the *controls* change.

**New layout, top to bottom:**
1. **Kind** — two `ToolbarToggle`s (or a `RadioButtonGroup`) for `Event` / `Reactive Variable`. Default: `Reactive Variable`. Choosing `Event` hides the type row (`AddToClassList("is-hidden")`) and pins the type to `EventViewModelSO`.
2. **Type** (reactive variables only) — a `Button` showing the current type's name; clicking it opens `MvvmTypeDropdown` via `Show(button.worldBound)`. Selecting a type sets `_selectedType` and refreshes the preview.
3. **Name** — a `TextField`. On every change, recompute the preview and the duplicate warning.
4. **Preview** — a read-only `Label` showing `<finalName>.asset` in `<folder>`, built with `MvvmAssetNameBuilder.GetAssetNameWithTypeNameAndName(_selectedType.Name, _nameField.value)`.
5. **Folder** — a read-only `TextField` plus a `Browse` `Button` (`EditorUtility.OpenFolderPanel`). Default: the folder selected in the Project window (the existing `GetSelectedFolderPath()` already does this — keep it).
6. **Warning label** — reused for both the duplicate warning and creation errors.
7. **Create** button.

**Folder browsing — handle all three outcomes explicitly:**
```csharp
        private void BrowseFolder()
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Select folder", _folderField.value, string.Empty);

            if (string.IsNullOrEmpty(absolutePath))
                return;

            string projectPath = GetProjectRelativePath(absolutePath);

            if (string.IsNullOrEmpty(projectPath))
            {
                ShowError("The folder must be inside the project.");
                return;
            }

            _folderField.SetValueWithoutNotify(projectPath);
            RefreshPreview();
        }

        private string GetProjectRelativePath(string absolutePath)
        {
            string normalizedPath = absolutePath.Replace("\\", "/");
            string dataPath = Application.dataPath;

            if (!normalizedPath.StartsWith(dataPath, StringComparison.Ordinal))
                return string.Empty;

            return "Assets" + normalizedPath.Substring(dataPath.Length);
        }
```
- Cancel → `OpenFolderPanel` returns an empty string → leave the folder unchanged, no error.
- Outside the project → `GetProjectRelativePath` returns empty → show the error, leave the folder unchanged.
- Inside the project → convert the absolute path to an `Assets/…` path. `Application.dataPath` is the absolute path of `Assets`, so this is the correct conversion. Needs `using UnityEngine;`.

**The duplicate warning must run against the FINAL suffixed name**, not the raw typed name — otherwise it compares apples to oranges against the existing asset names. So call `MvvmAssetNameBuilder` first, then `GetSimilarNamesWithName(finalName, _existingAssetNames)`.

**Guard the empty name before calling the builder**, because `GetAssetNameWithTypeNameAndName` throws on an empty name and the preview runs on every keystroke — including when the field is empty. An empty field must show an empty preview and no warning, not an exception spam loop.

- [ ] **Step 1: Rewrite the popup per the layout above**

Preserve: the `_existingAssetNames` cache built once in `CreateGUI`; the specific `catch (DirectoryNotFoundException)` / `catch (InvalidOperationException)` around `Create` that surface the message in the warning label; the `OnCreated` callback fired after a successful create; `Selection.activeObject` + `PingObject` on the created asset.

- [ ] **Step 2: Run the tests, confirm they PASS**

Expected: `total="52" passed="52"`, zero compile errors.

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Authoring/CreateMvvmAssetPopup.cs
git -C Packages/UP-MVVM add -A -- '*.meta'
git -C Packages/UP-MVVM commit -m "[U] Updated create popup with kind toggle, type dropdown, name preview and folder browser"
```

---

### Task 9: Version bump and README

**Files:**
- Modify: `package.json`
- Modify: `README.md`

- [ ] **Step 1: Bump the version**

Read `package.json` first. Change `"version": "1.31.0"` to `"1.32.0"` (minor — new behaviour, no breaking change). Do **not** touch `"unity": "6000.0"`.

- [ ] **Step 2: Update the README's MVVM Explorer section**

Amend the existing section to describe the new behaviour:
- the list is grouped by feature, derived from the type's `[CreateAssetMenu]` and falling back to the asset's folder;
- group headers collapse and expand;
- double-clicking a row selects and pings the asset in the Project window;
- `+ Create` asks for the kind (event or reactive variable), offers a searchable type picker, auto-suffixes the name with the type, and takes a browsed folder.

Keep the existing honest caveats (the duplicate filter is lexical and does not catch synonyms; `Unused` means "no reference found" and an SO loaded from code will read as unused; press **Rebuild Index** if results look stale).

Add one new caveat: **events are always grouped by folder**, because a single `EventViewModelSO` type is shared by every event asset, so `[CreateAssetMenu]` carries no feature information for them.

- [ ] **Step 3: Confirm `package.json` is still valid JSON, then run the tests**

Expected: `total="52" passed="52"`.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add package.json README.md
git -C Packages/UP-MVVM commit -m "[U] Updated package version and documented the reworked explorer list and create popup"
```

---

## Definition of Done

- [ ] **52/52 EditMode tests pass** in the batch testbed (33 baseline + 19 new), confirmed by an actual run, not asserted.
- [ ] Zero `error CS` in the testbed log.
- [ ] The list is grouped by feature, headers collapse/expand, and the reference count still shows per row.
- [ ] Events group by folder (never all under `MVVM`).
- [ ] The search field sits flush in its styled row with the filter beside it.
- [ ] Double-clicking a row selects and pings the asset in the Project window.
- [ ] `+ Create` shows a kind toggle, a searchable hierarchical type picker, a live suffixed-name preview, and a folder Browse button that rejects folders outside the project.
- [ ] `AssetListPanel` is deleted; nothing references it.
- [ ] All Unity-generated `.meta` files are committed.
