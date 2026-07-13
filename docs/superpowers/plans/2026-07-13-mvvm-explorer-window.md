# MVVM Explorer Window Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the two duplicated, slow MVVM searcher windows with a single `Tools/MVVM/Explorer` window that handles discovery, reference tracing, runtime debugging, and authoring of the ~1000 `EventViewModelSO` / `ReactiveVariableSO` assets.

**Architecture:** A UI Toolkit `EditorWindow` acts as a shell that composes three panels and owns no logic. The work lives in single-responsibility plain classes behind it. The core is a **reverse reference index**: rather than loading every asset and re-serializing its components (what the current windows do, and why they freeze), we text-scan the raw YAML of `.prefab` / `.unity` / `.asset` files for the target's GUID. It loads nothing, catches ScriptableObject→ScriptableObject references the current windows miss entirely, is cached to `Library/`, and is incrementally patched by an `AssetPostprocessor`.

**Tech Stack:** Unity 6000.x, C#, UI Toolkit (`ListView`, `TwoPaneSplitView`, `ToolbarSearchField`), Unity Test Framework + NUnit (EditMode).

**Spec:** [2026-07-13-mvvm-explorer-window-design.md](../specs/2026-07-13-mvvm-explorer-window-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **Repo:** all git commands run inside `Packages/UP-MVVM` (the project root is **not** a git repo). Branch: `dev/lalo/mvvmExplorerWindow`. Never switch branches.
- **Commit prefixes:** `[A]` add, `[U]` update, `[D]` delete, `[F]` fix. One package, one concern per commit.
- **YOU CANNOT RUN THE TESTS.** There is no test CLI. Tests run only in the Unity Editor's **Test Runner** window (`Window > General > Test Runner > EditMode`). Every "run the test" step is a **handoff to the user** — stop and ask them to run it and report the result. Do **not** claim a test passes without their confirmation.
- **Do NOT use the Unity MCP tools to verify anything.** The MCP is connected to a *different* Unity project; a clean console there is a false green.
- **Namespace:** `MVVM.CoreEditor` for all new editor code. **Assembly:** existing `Editor.MVVM.Core` (GUID `de9844047d89fc343b783902ee96b936`); runtime is `Runtime.MVVM.Core` (GUID `6924c379369b83e4d87549b6fdacec70`).
- **Code standards (CLAUDE.md), enforced in review:** one type per file; never abbreviate; no `var` outside `foreach`; **no `else`** (guard clauses / early return); no ternaries; braces **only** when the body is more than one line, and a single-line `if` body goes on the **next** line; no public fields (expose via `public string Name => _name;`); no `set` (use a `Set…()` method); a property must not compute; methods start with a verb and are prefixed by role (`Get…` / `Is…`/`Has…` / `Set…`), adding `With<Param>` when a parameter is essential; **throw, don't return null**; no bool parameters; no Singletons; `[SerializeField] private` on one line; self-documenting code, no explanatory comments.
- **Test naming:** `MethodName_WhatConditions_DoesWhat()`, AAA, **one assert per test**.
- **`.meta` files:** Unity generates them. After creating files, the user must let the Editor import them. Never hand-write or hand-edit a GUID.

---

### Task 1: Fix the ReactiveVariableSO play-mode teardown bug

The runtime pane's live value display depends on this callback path, so it is fixed before anything is built on it.

`ChangePlayMode` unsubscribes with `+=` instead of `-=`, *and* clears the guard flag — so the next `GetReactiveVariable()` attaches yet another handler. Handlers multiply on every play-mode cycle. `EventViewModelSO.Dispose` does the same teardown correctly with `-=`, which is how this was spotted.

**Files:**
- Modify: `Runtime/Core/InterfaceAdapters/ReactiveVariable/Engine/Core/ReactiveVariableSO.cs:59`

**Interfaces:**
- Consumes: nothing.
- Produces: a `ReactiveVariableSO<TValue>` whose `OnValueChangedEditorOnly` fires exactly once per value change after repeated play-mode cycles. Task 12 (`RuntimePanel`) relies on this.

**No unit test.** This is a private, editor-only `EditorApplication.playModeStateChanged` callback with no seam to inject a fake play-mode transition through. Building that seam would mean restructuring runtime code well beyond this fix. It is verified manually below instead. This is a deliberate, stated exception to TDD — do not silently skip the manual verification because it is inconvenient.

- [ ] **Step 1: Apply the one-line fix**

In `ReactiveVariableSO.cs`, in `ChangePlayMode`, change the `+=` to `-=`:

```csharp
#if UNITY_EDITOR
        private void ChangePlayMode(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingEditMode &&
                playModeStateChange != PlayModeStateChange.ExitingPlayMode) 
                return;
            
            _isSubscribedToPlayModeChanged = false;
            EditorApplication.playModeStateChanged -= ChangePlayMode;
            
            if(null != _reactiveVariable)
                _reactiveVariable.OnValueChangedEditorOnly -= UpdateValue;
            
            _reactiveVariable = null;
        }
#endif
```

- [ ] **Step 2: HANDOFF — ask the user to verify manually in the Editor**

Ask the user to perform exactly this and report back:

1. Select any `IntReactiveVariableSO` asset in the Project window.
2. Enter Play mode. In the Inspector's `Debug` section, confirm the `Value` field updates when the variable changes.
3. Exit Play mode. Repeat enter/exit **three times**.
4. Confirm the `Value` field still updates on the third cycle, and that no `MissingReferenceException` or duplicate-invocation warnings appear in the Console.

Expected: value updates correctly on every cycle. Before the fix, handlers accumulate each cycle.

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-MVVM add Runtime/Core/InterfaceAdapters/ReactiveVariable/Engine/Core/ReactiveVariableSO.cs
git -C Packages/UP-MVVM commit -m "[F] Fixed play mode teardown re-subscribing instead of unsubscribing"
```

---

### Task 2: Raise the Unity floor to 6000.0

The declared floor of `2020.3` is incompatible with this design: the 2020.3 list API is `ListView.itemHeight`, which no longer exists in Unity 6. No single `ListView` codebase compiles against both. The floor is vestigial — this workbench runs 6000.2 and the game runs 6000.5.

**Files:**
- Modify: `package.json:5-6`

**Interfaces:**
- Consumes: nothing.
- Produces: a package that may use Unity 6 APIs. Tasks 10–13 (UI Toolkit) depend on this.

- [ ] **Step 1: Edit package.json**

Change:
```json
  "unity": "2020.3",
  "unityRelease": "0b5",
```
to:
```json
  "unity": "6000.0",
```
(Delete the `unityRelease` line entirely — it pinned a 2020.3 beta release and is meaningless now.)

- [ ] **Step 2: HANDOFF — ask the user to confirm the package still resolves**

Ask the user to focus the Unity Editor, let it recompile, and confirm the Console shows **no** package-resolution errors and no warning that the package requires a newer Unity version.

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-MVVM add package.json
git -C Packages/UP-MVVM commit -m "[U] Updated minimum Unity version to 6000.0"
```

---

### Task 3: ReferenceIndex (+ EditMode test scaffolding)

The pure lookup structure at the heart of the tool. It performs **no** scanning — it is handed a map and answers questions about it. This task also creates the package's first `Tests/` folder, because this is the first thing that needs it.

**Files:**
- Create: `Tests/Editor/EditMode.Test.MVVM.asmdef`
- Create: `Editor/ReferenceIndex/ReferenceIndex.cs`
- Test: `Tests/Editor/ReferenceIndexTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `ReferenceIndex(Dictionary<string, List<string>> referencingPathsByGuid)` — throws `ArgumentNullException` on null.
  - `IReadOnlyList<string> GetReferencingPathsWithGuid(string guid)` — empty list for an unknown GUID, **never null**.
  - `bool HasReferencesWithGuid(string guid)`
  - `void AddReferenceWithGuid(string guid, string referencingPath)`
  - `void RemoveReferencingPath(string referencingPath)` — removes that path from **every** GUID entry.
  - `IReadOnlyCollection<string> GetIndexedGuids()`

  Tasks 7, 8, 9, 11 consume these exact signatures.

- [ ] **Step 1: Create the test assembly definition**

Create `Tests/Editor/EditMode.Test.MVVM.asmdef`:

```json
{
    "name": "EditMode.Test.MVVM",
    "rootNamespace": "MVVM.CoreEditor.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Runtime.MVVM.Core",
        "Editor.MVVM.Core"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Write the failing tests**

Create `Tests/Editor/ReferenceIndexTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using MVVM.CoreEditor;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class ReferenceIndexTests
    {
        private const string PlayerDiedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string EnemySpawnedGuid = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private const string HudPrefabPath = "Assets/Prefabs/Hud.prefab";
        private const string MenuPrefabPath = "Assets/Prefabs/Menu.prefab";

        [Test]
        public void Constructor_WhenMapIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReferenceIndex(null));
        }

        [Test]
        public void GetReferencingPathsWithGuid_WhenGuidIsUnknown_ReturnsEmptyList()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.IsEmpty(referencingPaths);
        }

        [Test]
        public void GetReferencingPathsWithGuid_WhenGuidHasOneReference_ReturnsThatPath()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.AreEqual(new[] { HudPrefabPath }, referencingPaths);
        }

        [Test]
        public void HasReferencesWithGuid_WhenGuidIsUnknown_ReturnsFalse()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();

            bool hasReferences = referenceIndex.HasReferencesWithGuid(PlayerDiedGuid);

            Assert.IsFalse(hasReferences);
        }

        [Test]
        public void HasReferencesWithGuid_WhenGuidHasOneReference_ReturnsTrue()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            bool hasReferences = referenceIndex.HasReferencesWithGuid(PlayerDiedGuid);

            Assert.IsTrue(hasReferences);
        }

        [Test]
        public void AddReferenceWithGuid_WhenPathIsAlreadyIndexed_DoesNotDuplicateIt()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.AreEqual(1, referencingPaths.Count);
        }

        [Test]
        public void RemoveReferencingPath_WhenPathReferencesSeveralGuids_RemovesItFromEveryGuid()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, MenuPrefabPath);

            referenceIndex.RemoveReferencingPath(HudPrefabPath);

            Assert.AreEqual(new[] { MenuPrefabPath }, referenceIndex.GetReferencingPathsWithGuid(EnemySpawnedGuid));
        }

        [Test]
        public void RemoveReferencingPath_WhenPathIsTheOnlyReference_LeavesGuidWithNoReferences()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            referenceIndex.RemoveReferencingPath(HudPrefabPath);

            Assert.IsFalse(referenceIndex.HasReferencesWithGuid(PlayerDiedGuid));
        }

        [Test]
        public void GetIndexedGuids_WhenTwoGuidsAreIndexed_ReturnsBoth()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, MenuPrefabPath);

            IReadOnlyCollection<string> indexedGuids = referenceIndex.GetIndexedGuids();

            Assert.AreEqual(2, indexedGuids.Count);
        }

        private ReferenceIndex CreateEmptyIndex()
        {
            return new ReferenceIndex(new Dictionary<string, List<string>>());
        }
    }
}
```

- [ ] **Step 3: HANDOFF — ask the user to run the tests and confirm they FAIL**

Ask the user to open `Window > General > Test Runner > EditMode`, run `EditMode.Test.MVVM`, and report the result.

Expected: compile error — `ReferenceIndex` does not exist. That is the correct failing state.

- [ ] **Step 4: Write the implementation**

Create `Editor/ReferenceIndex/ReferenceIndex.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace MVVM.CoreEditor
{
    public class ReferenceIndex
    {
        private readonly Dictionary<string, List<string>> _referencingPathsByGuid;

        public ReferenceIndex(Dictionary<string, List<string>> referencingPathsByGuid)
        {
            if (ReferenceEquals(referencingPathsByGuid, null))
                throw new ArgumentNullException(nameof(referencingPathsByGuid));

            _referencingPathsByGuid = referencingPathsByGuid;
        }

        public IReadOnlyList<string> GetReferencingPathsWithGuid(string guid)
        {
            if (!_referencingPathsByGuid.ContainsKey(guid))
                return Array.Empty<string>();

            return _referencingPathsByGuid[guid];
        }

        public bool HasReferencesWithGuid(string guid)
        {
            return GetReferencingPathsWithGuid(guid).Count > 0;
        }

        public void AddReferenceWithGuid(string guid, string referencingPath)
        {
            if (!_referencingPathsByGuid.ContainsKey(guid))
                _referencingPathsByGuid.Add(guid, new List<string>());

            if (_referencingPathsByGuid[guid].Contains(referencingPath))
                return;

            _referencingPathsByGuid[guid].Add(referencingPath);
        }

        public void RemoveReferencingPath(string referencingPath)
        {
            foreach (KeyValuePair<string, List<string>> referencingPathsForGuid in _referencingPathsByGuid)
                referencingPathsForGuid.Value.Remove(referencingPath);
        }

        public IReadOnlyCollection<string> GetIndexedGuids()
        {
            return _referencingPathsByGuid.Keys;
        }
    }
}
```

- [ ] **Step 5: HANDOFF — ask the user to run the tests and confirm they PASS**

Ask the user to re-run `EditMode.Test.MVVM` in the Test Runner.

Expected: 9/9 PASS. Do not proceed until the user confirms.

- [ ] **Step 6: Commit**

```bash
git -C Packages/UP-MVVM add Tests Editor/ReferenceIndex
git -C Packages/UP-MVVM commit -m "[A] Added reference index and edit mode test assembly"
```

---

### Task 4: YamlGuidExtractor

Pulls every referenced GUID out of a raw Unity YAML string. Pure — takes a string, returns GUIDs. No file IO, no `AssetDatabase`, fully testable headless.

Unity serializes an asset reference as `{fileID: 11400000, guid: <32 hex chars>, type: 2}`. Extracting the GUIDs textually is what lets us find references without loading a single asset — including ScriptableObject→ScriptableObject references, which the old windows miss entirely.

**Files:**
- Create: `Editor/ReferenceIndex/YamlGuidExtractor.cs`
- Test: `Tests/Editor/YamlGuidExtractorTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `IReadOnlyCollection<string> GetGuidsFromYaml(string yaml)` — distinct GUIDs, empty collection for null/empty input, **never null**. Tasks 7, 8 consume this.

- [ ] **Step 1: Write the failing tests**

Create `Tests/Editor/YamlGuidExtractorTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class YamlGuidExtractorTests
    {
        private const string PlayerDiedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ScriptGuid = "cccccccccccccccccccccccccccccccc";

        [Test]
        public void GetGuidsFromYaml_WhenYamlIsNull_ReturnsEmptyCollection()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(null);

            Assert.IsEmpty(guids);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasNoReferences_ReturnsEmptyCollection()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml("m_Name: Hud\nm_Enabled: 1\n");

            Assert.IsEmpty(guids);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasOneReference_ReturnsThatGuid()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string yaml = "  _eventViewModelSo: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(yaml);

            Assert.AreEqual(new[] { PlayerDiedGuid }, guids.ToArray());
        }

        [Test]
        public void GetGuidsFromYaml_WhenTheSameGuidAppearsTwice_ReturnsItOnce()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string reference = "  _event: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(reference + reference);

            Assert.AreEqual(1, guids.Count);
        }

        [Test]
        public void GetGuidsFromYaml_WhenYamlHasScriptAndAssetReferences_ReturnsBoth()
        {
            YamlGuidExtractor yamlGuidExtractor = new YamlGuidExtractor();
            string yaml =
                "  m_Script: {fileID: 11500000, guid: " + ScriptGuid + ", type: 3}\n" +
                "  _eventViewModelSo: {fileID: 11400000, guid: " + PlayerDiedGuid + ", type: 2}\n";

            IReadOnlyCollection<string> guids = yamlGuidExtractor.GetGuidsFromYaml(yaml);

            Assert.AreEqual(2, guids.Count);
        }
    }
}
```

Note on the last test: the `m_Script` GUID is deliberately returned too. Filtering it out here would be wrong — the extractor's job is "which GUIDs does this YAML mention". Only GUIDs belonging to MVVM SO assets are ever *looked up*, so extra GUIDs in the index are harmless, and excluding script GUIDs would mean the extractor needed to know what a script is.

- [ ] **Step 2: HANDOFF — ask the user to run the tests and confirm they FAIL**

Expected: compile error — `YamlGuidExtractor` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Editor/ReferenceIndex/YamlGuidExtractor.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MVVM.CoreEditor
{
    public class YamlGuidExtractor
    {
        private static readonly Regex GuidPattern =
            new Regex(@"guid:\s*([0-9a-f]{32})", RegexOptions.Compiled);

        public IReadOnlyCollection<string> GetGuidsFromYaml(string yaml)
        {
            if (string.IsNullOrEmpty(yaml))
                return Array.Empty<string>();

            HashSet<string> guids = new HashSet<string>();

            foreach (Match guidMatch in GuidPattern.Matches(yaml))
                guids.Add(guidMatch.Groups[1].Value);

            return guids;
        }
    }
}
```

- [ ] **Step 4: HANDOFF — ask the user to run the tests and confirm they PASS**

Expected: 5/5 PASS (plus the 9 from Task 3 = 14 total).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/ReferenceIndex/YamlGuidExtractor.cs Tests/Editor/YamlGuidExtractorTests.cs
git -C Packages/UP-MVVM commit -m "[A] Added yaml guid extractor"
```

---

### Task 5: DuplicateNameFinder

Groups lexically near-identical asset names so the Explorer can warn about duplicates. Pure — takes names, returns groups.

**Read this before implementing:** the heuristic normalizes (lowercase, strip a leading `On`, strip non-alphanumerics) then groups names within a Levenshtein distance of 2. It catches **prefix and typo/plural** duplicates (`PlayerDied` / `OnPlayerDied` / `PlayersDied`). It does **not** catch **synonym** duplicates — `playerdied` → `playerdeath` is distance **4**, and lowering the threshold far enough to group them would sweep in unrelated names. One of the tests below pins this limitation on purpose. **Do not "fix" it by raising the threshold.**

**Files:**
- Create: `Editor/Duplicates/DuplicateNameFinder.cs`
- Test: `Tests/Editor/DuplicateNameFinderTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `IReadOnlyList<IReadOnlyList<string>> GetDuplicateGroups(IReadOnlyList<string> names)` — only groups of 2+ are returned; unique names are excluded entirely. Task 10 (`AssetListPanel`) consumes this.

- [ ] **Step 1: Write the failing tests**

Create `Tests/Editor/DuplicateNameFinderTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class DuplicateNameFinderTests
    {
        [Test]
        public void GetDuplicateGroups_WhenNamesDifferOnlyByOnPrefix_GroupsThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "OnPlayerDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(1, duplicateGroups.Count);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesDifferByOneCharacter_GroupsThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "PlayersDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(2, duplicateGroups[0].Count);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesAreUnrelated_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "EnemySpawned" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesUseDifferentWordsForTheSameIdea_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "PlayerDeath" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenOnlyOneNameIsGiven_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenThreeNamesMatch_ReturnsThemInOneGroup()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "OnPlayerDied", "PlayersDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(3, duplicateGroups[0].Count);
        }
    }
}
```

The fourth test is the limitation pin. It asserts `PlayerDied` and `PlayerDeath` are **not** grouped. That is intended behaviour, not a bug.

- [ ] **Step 2: HANDOFF — ask the user to run the tests and confirm they FAIL**

Expected: compile error — `DuplicateNameFinder` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Editor/Duplicates/DuplicateNameFinder.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace MVVM.CoreEditor
{
    public class DuplicateNameFinder
    {
        private const int MaximumDistance = 2;
        private const string IgnoredPrefix = "on";

        public IReadOnlyList<IReadOnlyList<string>> GetDuplicateGroups(IReadOnlyList<string> names)
        {
            if (ReferenceEquals(names, null))
                throw new ArgumentNullException(nameof(names));

            List<IReadOnlyList<string>> duplicateGroups = new List<IReadOnlyList<string>>();
            HashSet<int> groupedIndexes = new HashSet<int>();

            for (int currentIndex = 0; currentIndex < names.Count; currentIndex++)
            {
                if (groupedIndexes.Contains(currentIndex))
                    continue;

                List<string> group = GetGroupForIndex(names, currentIndex, groupedIndexes);

                if (group.Count < 2)
                    continue;

                duplicateGroups.Add(group);
            }

            return duplicateGroups;
        }

        private List<string> GetGroupForIndex(IReadOnlyList<string> names, int currentIndex, HashSet<int> groupedIndexes)
        {
            List<string> group = new List<string> { names[currentIndex] };
            string currentName = Normalize(names[currentIndex]);

            for (int otherIndex = currentIndex + 1; otherIndex < names.Count; otherIndex++)
            {
                if (groupedIndexes.Contains(otherIndex))
                    continue;

                if (GetDistance(currentName, Normalize(names[otherIndex])) > MaximumDistance)
                    continue;

                group.Add(names[otherIndex]);
                groupedIndexes.Add(otherIndex);
            }

            if (group.Count > 1)
                groupedIndexes.Add(currentIndex);

            return group;
        }

        private string Normalize(string name)
        {
            StringBuilder normalized = new StringBuilder();

            foreach (char character in name.ToLowerInvariant())
            {
                if (!char.IsLetterOrDigit(character))
                    continue;

                normalized.Append(character);
            }

            return RemoveIgnoredPrefix(normalized.ToString());
        }

        private string RemoveIgnoredPrefix(string name)
        {
            if (!name.StartsWith(IgnoredPrefix, StringComparison.Ordinal))
                return name;

            return name.Substring(IgnoredPrefix.Length);
        }

        private int GetDistance(string firstName, string secondName)
        {
            int[,] distances = new int[firstName.Length + 1, secondName.Length + 1];

            for (int firstIndex = 0; firstIndex <= firstName.Length; firstIndex++)
                distances[firstIndex, 0] = firstIndex;

            for (int secondIndex = 0; secondIndex <= secondName.Length; secondIndex++)
                distances[0, secondIndex] = secondIndex;

            for (int firstIndex = 1; firstIndex <= firstName.Length; firstIndex++)
            {
                for (int secondIndex = 1; secondIndex <= secondName.Length; secondIndex++)
                {
                    int substitutionCost = GetSubstitutionCost(firstName[firstIndex - 1], secondName[secondIndex - 1]);

                    distances[firstIndex, secondIndex] = Math.Min(
                        Math.Min(distances[firstIndex - 1, secondIndex] + 1, distances[firstIndex, secondIndex - 1] + 1),
                        distances[firstIndex - 1, secondIndex - 1] + substitutionCost);
                }
            }

            return distances[firstName.Length, secondName.Length];
        }

        private int GetSubstitutionCost(char firstCharacter, char secondCharacter)
        {
            if (firstCharacter == secondCharacter)
                return 0;

            return 1;
        }
    }
}
```

- [ ] **Step 4: HANDOFF — ask the user to run the tests and confirm they PASS**

Expected: 6/6 PASS (20 total).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Duplicates Tests/Editor/DuplicateNameFinderTests.cs
git -C Packages/UP-MVVM commit -m "[A] Added duplicate name finder"
```

---

### Task 6: MvvmAssetRepository

Enumerates the MVVM SO assets. A thin `AssetDatabase` wrapper — deliberately dumb, so everything worth testing lives in Tasks 3–5.

**Files:**
- Create: `Editor/Assets/MvvmAsset.cs`
- Create: `Editor/Assets/MvvmAssetRepository.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `MvvmAsset` with `string Guid`, `string Path`, `string Name`, `string TypeName`, `bool IsEvent` (all read-only, expression-bodied).
  - `MvvmAssetRepository.GetAllAssets()` → `IReadOnlyList<MvvmAsset>`, sorted by name.

  Tasks 10, 11, 12, 13 consume these.

- [ ] **Step 1: Write MvvmAsset**

Create `Editor/Assets/MvvmAsset.cs`:

```csharp
namespace MVVM.CoreEditor
{
    public class MvvmAsset
    {
        private readonly string _guid;
        private readonly string _path;
        private readonly string _name;
        private readonly string _typeName;
        private readonly bool _isEvent;

        public string Guid => _guid;
        public string Path => _path;
        public string Name => _name;
        public string TypeName => _typeName;
        public bool IsEvent => _isEvent;

        public MvvmAsset(string guid, string path, string name, string typeName, bool isEvent)
        {
            _guid = guid;
            _path = path;
            _name = name;
            _typeName = typeName;
            _isEvent = isEvent;
        }
    }
}
```

- [ ] **Step 2: Write MvvmAssetRepository**

Create `Editor/Assets/MvvmAssetRepository.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class MvvmAssetRepository
    {
        private const string EventFilter = "t:EventViewModelSO";
        private const string ReactiveVariableFilter = "t:BaseReactiveVariableSO";

        public IReadOnlyList<MvvmAsset> GetAllAssets()
        {
            List<MvvmAsset> assets = new List<MvvmAsset>();

            assets.AddRange(GetAssetsWithFilter(EventFilter));
            assets.AddRange(GetAssetsWithFilter(ReactiveVariableFilter));

            return assets.OrderBy(asset => asset.Name).ToList();
        }

        private IEnumerable<MvvmAsset> GetAssetsWithFilter(string filter)
        {
            string[] guids = AssetDatabase.FindAssets(filter);

            foreach (string guid in guids)
            {
                MvvmAsset asset = GetAssetWithGuid(guid);

                if (ReferenceEquals(asset, null))
                    continue;

                yield return asset;
            }
        }

        private MvvmAsset GetAssetWithGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (scriptableObject == null)
                return null;

            bool isEvent = scriptableObject is EventViewModelSO;

            return new MvvmAsset(guid, path, scriptableObject.name, scriptableObject.GetType().Name, isEvent);
        }
    }
}
```

- [ ] **Step 3: HANDOFF — ask the user to confirm it compiles**

Ask the user to focus the Editor, let it recompile, and confirm the Console is clean.

Note: `GetAssetWithGuid` returns `null` when the asset fails to load, which reads as a violation of "throw, don't return null". It is not: a GUID whose asset will not load is a **valid absence** during a project-wide sweep (a broken or mid-import asset), and the caller skips it. Throwing here would make one corrupt asset break the whole window.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Assets
git -C Packages/UP-MVVM commit -m "[A] Added mvvm asset repository"
```

---

### Task 7: ReferenceIndexBuilder

Builds the index by scanning YAML, and caches it to `Library/`. Chunked across `EditorApplication.update` ticks behind a cancellable progress bar so the Editor stays responsive over ~1000 assets.

**Files:**
- Create: `Editor/ReferenceIndex/ReferenceIndexBuilder.cs`
- Create: `Editor/ReferenceIndex/ReferenceIndexCache.cs`

**Interfaces:**
- Consumes: `ReferenceIndex` (Task 3), `YamlGuidExtractor` (Task 4).
- Produces:
  - `ReferenceIndexCache.Load()` → `ReferenceIndex` or `null` when missing/stale.
  - `ReferenceIndexCache.Save(ReferenceIndex referenceIndex)`
  - `ReferenceIndexBuilder.BuildAsync(Action<ReferenceIndex> onBuilt)` — chunked; invokes `onBuilt` when finished.
  - `ReferenceIndexBuilder.SetIndexForPath(string assetPath, ReferenceIndex referenceIndex)` — re-indexes one asset. Task 8 uses this. (Named `Set…`, not `Get…`: it mutates and returns nothing.)

- [ ] **Step 1: Write ReferenceIndexCache**

Create `Editor/ReferenceIndex/ReferenceIndexCache.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexCache
    {
        private const int FormatVersion = 1;
        private const string CacheFilePath = "Library/MvvmReferenceIndex.json";

        public ReferenceIndex Load()
        {
            if (!File.Exists(CacheFilePath))
                return null;

            ReferenceIndexCacheData cacheData = JsonUtility.FromJson<ReferenceIndexCacheData>(
                File.ReadAllText(CacheFilePath));

            if (ReferenceEquals(cacheData, null))
                return null;

            if (cacheData.formatVersion != FormatVersion)
                return null;

            return new ReferenceIndex(GetMapFromEntries(cacheData.entries));
        }

        public void Save(ReferenceIndex referenceIndex)
        {
            ReferenceIndexCacheData cacheData = new ReferenceIndexCacheData();
            cacheData.formatVersion = FormatVersion;
            cacheData.entries = GetEntriesFromIndex(referenceIndex);

            File.WriteAllText(CacheFilePath, JsonUtility.ToJson(cacheData));
        }

        private Dictionary<string, List<string>> GetMapFromEntries(List<ReferenceIndexCacheEntry> entries)
        {
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();

            foreach (ReferenceIndexCacheEntry entry in entries)
                map[entry.guid] = entry.referencingPaths;

            return map;
        }

        private List<ReferenceIndexCacheEntry> GetEntriesFromIndex(ReferenceIndex referenceIndex)
        {
            List<ReferenceIndexCacheEntry> entries = new List<ReferenceIndexCacheEntry>();

            foreach (string guid in referenceIndex.GetIndexedGuids())
            {
                ReferenceIndexCacheEntry entry = new ReferenceIndexCacheEntry();
                entry.guid = guid;
                entry.referencingPaths = new List<string>(referenceIndex.GetReferencingPathsWithGuid(guid));

                entries.Add(entry);
            }

            return entries;
        }
    }

    [Serializable]
    public class ReferenceIndexCacheData
    {
        public int formatVersion;
        public List<ReferenceIndexCacheEntry> entries = new List<ReferenceIndexCacheEntry>();
    }

    [Serializable]
    public class ReferenceIndexCacheEntry
    {
        public string guid;
        public List<string> referencingPaths = new List<string>();
    }
}
```

`ReferenceIndexCacheData` / `ReferenceIndexCacheEntry` use public fields and live in the same file as `ReferenceIndexCache`, breaking two package rules. Both are forced by `JsonUtility`, which only serializes public fields on `[Serializable]` types. They are DTOs with no behaviour. If a reviewer objects, split them into their own files — but the public fields must stay.

- [ ] **Step 2: Write ReferenceIndexBuilder**

Create `Editor/ReferenceIndex/ReferenceIndexBuilder.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexBuilder
    {
        private const int AssetsPerTick = 50;
        private static readonly string[] ScannedExtensions = { ".prefab", ".unity", ".asset" };

        private readonly YamlGuidExtractor _yamlGuidExtractor = new YamlGuidExtractor();
        private readonly ReferenceIndexCache _referenceIndexCache = new ReferenceIndexCache();

        private ReferenceIndex _referenceIndex;
        private List<string> _pendingPaths;
        private int _nextPathIndex;
        private Action<ReferenceIndex> _onBuilt;

        public void BuildAsync(Action<ReferenceIndex> onBuilt)
        {
            _onBuilt = onBuilt;
            _referenceIndex = new ReferenceIndex(new Dictionary<string, List<string>>());
            _pendingPaths = GetScannablePaths();
            _nextPathIndex = 0;

            EditorApplication.update += BuildNextChunk;
        }

        public void SetIndexForPath(string assetPath, ReferenceIndex referenceIndex)
        {
            referenceIndex.RemoveReferencingPath(assetPath);

            if (!IsScannable(assetPath))
                return;

            foreach (string guid in GetGuidsInFileWithPath(assetPath))
                referenceIndex.AddReferenceWithGuid(guid, assetPath);
        }

        private List<string> GetScannablePaths()
        {
            return AssetDatabase.GetAllAssetPaths().Where(IsScannable).ToList();
        }

        private bool IsScannable(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) &&
                !assetPath.StartsWith("Packages/", StringComparison.Ordinal))
                return false;

            return ScannedExtensions.Contains(Path.GetExtension(assetPath));
        }

        private IReadOnlyCollection<string> GetGuidsInFileWithPath(string assetPath)
        {
            if (!File.Exists(assetPath))
                return Array.Empty<string>();

            try
            {
                return _yamlGuidExtractor.GetGuidsFromYaml(File.ReadAllText(assetPath));
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"MVVM Explorer could not read {assetPath}: {exception.Message}");
                return Array.Empty<string>();
            }
        }

        private void BuildNextChunk()
        {
            int lastPathIndex = Math.Min(_nextPathIndex + AssetsPerTick, _pendingPaths.Count);

            for (int pathIndex = _nextPathIndex; pathIndex < lastPathIndex; pathIndex++)
                IndexPath(_pendingPaths[pathIndex]);

            _nextPathIndex = lastPathIndex;

            if (IsCancelledByUser())
                return;

            if (_nextPathIndex < _pendingPaths.Count)
                return;

            FinishBuild();
        }

        private void IndexPath(string assetPath)
        {
            foreach (string guid in GetGuidsInFileWithPath(assetPath))
                _referenceIndex.AddReferenceWithGuid(guid, assetPath);
        }

        private bool IsCancelledByUser()
        {
            float progress = (float)_nextPathIndex / _pendingPaths.Count;

            if (!EditorUtility.DisplayCancelableProgressBar(
                    "MVVM Explorer", $"Indexing references ({_nextPathIndex}/{_pendingPaths.Count})", progress))
                return false;

            StopBuild();
            _onBuilt?.Invoke(_referenceIndex);

            return true;
        }

        private void FinishBuild()
        {
            StopBuild();
            _referenceIndexCache.Save(_referenceIndex);
            _onBuilt?.Invoke(_referenceIndex);
        }

        private void StopBuild()
        {
            EditorApplication.update -= BuildNextChunk;
            EditorUtility.ClearProgressBar();
        }
    }
}
```

The `catch (IOException)` catches a **specific** exception and **logs** it. This is not the blanket `catch { continue; }` from the old window — a failure to read one file must not kill the sweep, but it must be visible.

Cancelling leaves a **partial** index and does not save it to cache, so the next open rebuilds.

- [ ] **Step 3: HANDOFF — ask the user to confirm it compiles**

Ask the user to focus the Editor and confirm the Console is clean. No behaviour is observable yet; the window arrives in Task 10.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/ReferenceIndex/ReferenceIndexBuilder.cs Editor/ReferenceIndex/ReferenceIndexCache.cs
git -C Packages/UP-MVVM commit -m "[A] Added reference index builder and cache"
```

---

### Task 8: ReferenceIndexUpdater

Keeps the index current while the Editor is open, so the expensive full build happens once.

**Files:**
- Create: `Editor/ReferenceIndex/ReferenceIndexUpdater.cs`
- Create: `Editor/ReferenceIndex/ReferenceIndexProvider.cs`

**Interfaces:**
- Consumes: `ReferenceIndex` (3), `ReferenceIndexBuilder` + `ReferenceIndexCache` (7).
- Produces:
  - `ReferenceIndexProvider.GetIndex(Action<ReferenceIndex> onReady)` — cached index if present, otherwise triggers a build.
  - `ReferenceIndexProvider.Rebuild(Action<ReferenceIndex> onReady)` — forces a full rebuild (the `Rebuild Index` toolbar button, Task 10).
  - `ReferenceIndexProvider.HasIndex()`

- [ ] **Step 1: Write ReferenceIndexProvider**

Create `Editor/ReferenceIndex/ReferenceIndexProvider.cs`:

```csharp
using System;

namespace MVVM.CoreEditor
{
    public static class ReferenceIndexProvider
    {
        private static ReferenceIndex _referenceIndex;
        private static readonly ReferenceIndexCache Cache = new ReferenceIndexCache();

        public static bool HasIndex()
        {
            return !ReferenceEquals(_referenceIndex, null);
        }

        public static ReferenceIndex GetLoadedIndex()
        {
            if (ReferenceEquals(_referenceIndex, null))
                throw new InvalidOperationException("The reference index is not built yet. Call GetIndex first.");

            return _referenceIndex;
        }

        public static void GetIndex(Action<ReferenceIndex> onReady)
        {
            if (HasIndex())
            {
                onReady(_referenceIndex);
                return;
            }

            _referenceIndex = Cache.Load();

            if (HasIndex())
            {
                onReady(_referenceIndex);
                return;
            }

            Rebuild(onReady);
        }

        public static void Rebuild(Action<ReferenceIndex> onReady)
        {
            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            referenceIndexBuilder.BuildAsync(builtIndex =>
            {
                _referenceIndex = builtIndex;
                onReady(builtIndex);
            });
        }
    }
}
```

This is `static` state, not a Singleton class — it holds one in-memory cache for the Editor session. Do **not** turn it into a `Singleton<T>`; the package forbids new Singleton usage.

- [ ] **Step 2: Write ReferenceIndexUpdater**

Create `Editor/ReferenceIndex/ReferenceIndexUpdater.cs`:

```csharp
using UnityEditor;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexUpdater : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ReferenceIndexProvider.HasIndex())
                return;

            ReferenceIndex referenceIndex = ReferenceIndexProvider.GetLoadedIndex();
            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            foreach (string deletedAsset in deletedAssets)
                referenceIndex.RemoveReferencingPath(deletedAsset);

            foreach (string movedFromAssetPath in movedFromAssetPaths)
                referenceIndex.RemoveReferencingPath(movedFromAssetPath);

            foreach (string importedAsset in importedAssets)
                referenceIndexBuilder.SetIndexForPath(importedAsset, referenceIndex);

            foreach (string movedAsset in movedAssets)
                referenceIndexBuilder.SetIndexForPath(movedAsset, referenceIndex);
        }
    }
}
```

The guard matters: when no index is loaded, this does nothing. Never trigger a full build from an asset import — that would freeze the Editor on every save.

- [ ] **Step 3: HANDOFF — ask the user to confirm it compiles**

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/ReferenceIndex/ReferenceIndexUpdater.cs Editor/ReferenceIndex/ReferenceIndexProvider.cs
git -C Packages/UP-MVVM commit -m "[A] Added reference index updater and provider"
```

---

### Task 9: OpenSceneReferenceScanner

The YAML scan reads what is on **disk**, so it cannot see unsaved edits in an open scene. This walks the loaded scenes live and its results are merged over the index.

This retains the one genuinely working piece of the old window — the `EditorSceneManager` traversal — while dropping the project-wide `LoadMainAssetAtPath` sweep that made it slow.

**Files:**
- Create: `Editor/ReferenceIndex/OpenSceneReferenceScanner.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `IReadOnlyList<Component> GetReferencingComponentsWithAsset(Object targetAsset)`. Task 11 consumes this.

- [ ] **Step 1: Write the scanner**

Create `Editor/ReferenceIndex/OpenSceneReferenceScanner.cs`:

```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class OpenSceneReferenceScanner
    {
        public IReadOnlyList<Component> GetReferencingComponentsWithAsset(Object targetAsset)
        {
            List<Component> referencingComponents = new List<Component>();

            foreach (GameObject sceneGameObject in GetAllGameObjectsInOpenScenes())
                AddReferencingComponents(sceneGameObject, targetAsset, referencingComponents);

            return referencingComponents;
        }

        private void AddReferencingComponents(
            GameObject sceneGameObject, Object targetAsset, List<Component> referencingComponents)
        {
            foreach (Component component in sceneGameObject.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                if (!HasReferenceToAsset(component, targetAsset))
                    continue;

                referencingComponents.Add(component);
            }
        }

        private bool HasReferenceToAsset(Component component, Object targetAsset)
        {
            SerializedObject serializedComponent = new SerializedObject(component);
            SerializedProperty serializedProperty = serializedComponent.GetIterator();

            while (serializedProperty.NextVisible(true))
            {
                if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (serializedProperty.objectReferenceValue != targetAsset)
                    continue;

                return true;
            }

            return false;
        }

        private IEnumerable<GameObject> GetAllGameObjectsInOpenScenes()
        {
            for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(sceneIndex);

                if (!scene.isLoaded)
                    continue;

                foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (GameObject gameObject in GetGameObjectWithChildren(rootGameObject))
                        yield return gameObject;
                }
            }
        }

        private IEnumerable<GameObject> GetGameObjectWithChildren(GameObject parentGameObject)
        {
            yield return parentGameObject;

            foreach (Transform childTransform in parentGameObject.transform)
            {
                foreach (GameObject childGameObject in GetGameObjectWithChildren(childTransform.gameObject))
                    yield return childGameObject;
            }
        }
    }
}
```

- [ ] **Step 2: HANDOFF — ask the user to confirm it compiles**

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-MVVM add Editor/ReferenceIndex/OpenSceneReferenceScanner.cs
git -C Packages/UP-MVVM commit -m "[A] Added open scene reference scanner"
```

---

### Task 10: MvvmExplorerWindow shell + AssetListPanel

The window and the discovery pane. First task with visible behaviour — the user can finally see something.

**Files:**
- Create: `Editor/Explorer/MvvmExplorerWindow.cs`
- Create: `Editor/Explorer/AssetListPanel.cs`

**Interfaces:**
- Consumes: `MvvmAssetRepository`, `MvvmAsset` (6); `ReferenceIndexProvider`, `ReferenceIndex` (3, 8); `DuplicateNameFinder` (5).
- Produces:
  - `MvvmExplorerWindow.ShowWindow()` and `MvvmExplorerWindow.ShowWindowWithAsset(Object asset)` — Task 14 calls the latter.
  - `AssetListPanel.OnAssetSelected` (`Action<MvvmAsset>`) — Tasks 11, 12 subscribe.
  - `AssetListPanel.SelectAssetWithGuid(string guid)`

- [ ] **Step 1: Write AssetListPanel**

Create `Editor/Explorer/AssetListPanel.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class AssetListPanel
    {
        private const int RowHeight = 20;

        private readonly VisualElement _root = new VisualElement();
        private readonly ListView _listView = new ListView();
        private readonly ToolbarSearchField _searchField = new ToolbarSearchField();
        private readonly EnumField _filterField = new EnumField(AssetListFilter.All);
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();

        private IReadOnlyList<MvvmAsset> _allAssets = Array.Empty<MvvmAsset>();
        private List<MvvmAsset> _visibleAssets = new List<MvvmAsset>();
        private ReferenceIndex _referenceIndex;
        private HashSet<string> _duplicateNames = new HashSet<string>();

        public VisualElement Root => _root;
        public Action<MvvmAsset> OnAssetSelected { get; set; }

        public AssetListPanel()
        {
            _root.style.flexGrow = 1;

            _searchField.RegisterValueChangedCallback(searchChange => RefreshVisibleAssets());
            _filterField.RegisterValueChangedCallback(filterChange => RefreshVisibleAssets());

            _listView.fixedItemHeight = RowHeight;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            _listView.selectionType = SelectionType.Single;
            _listView.makeItem = CreateRow;
            _listView.bindItem = BindRow;
            _listView.style.flexGrow = 1;
            _listView.selectionChanged += RaiseAssetSelected;

            _root.Add(_searchField);
            _root.Add(_filterField);
            _root.Add(_listView);
        }

        public void SetAssets(IReadOnlyList<MvvmAsset> assets, ReferenceIndex referenceIndex)
        {
            _allAssets = assets;
            _referenceIndex = referenceIndex;
            _duplicateNames = GetDuplicateNames(assets);

            RefreshVisibleAssets();
        }

        public void SelectAssetWithGuid(string guid)
        {
            int assetIndex = _visibleAssets.FindIndex(asset => asset.Guid == guid);

            if (assetIndex < 0)
                return;

            _listView.SetSelection(assetIndex);
            _listView.ScrollToItem(assetIndex);
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

        private void RefreshVisibleAssets()
        {
            _visibleAssets = _allAssets.Where(IsAssetVisible).ToList();

            _listView.itemsSource = _visibleAssets;
            _listView.Rebuild();
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
                return !_referenceIndex.HasReferencesWithGuid(asset.Guid);

            return _duplicateNames.Contains(asset.Name);
        }

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            Label nameLabel = new Label();
            nameLabel.name = "name";
            nameLabel.style.flexGrow = 1;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            Label referenceCountLabel = new Label();
            referenceCountLabel.name = "referenceCount";
            referenceCountLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            referenceCountLabel.style.opacity = 0.6f;

            row.Add(nameLabel);
            row.Add(referenceCountLabel);

            return row;
        }

        private void BindRow(VisualElement row, int assetIndex)
        {
            MvvmAsset asset = _visibleAssets[assetIndex];

            row.Q<Label>("name").text = asset.Name;
            row.Q<Label>("referenceCount").text =
                _referenceIndex.GetReferencingPathsWithGuid(asset.Guid).Count.ToString();
        }

        private void RaiseAssetSelected(IEnumerable<object> selectedItems)
        {
            MvvmAsset selectedAsset = selectedItems.FirstOrDefault() as MvvmAsset;

            if (ReferenceEquals(selectedAsset, null))
                return;

            OnAssetSelected?.Invoke(selectedAsset);
        }
    }
}
```

The `OnAssetSelected { get; set; }` `set` is the one allowed exception in the package rules: an event on a type used as an interface boundary.

- [ ] **Step 2: Write the filter enum**

Create `Editor/Explorer/AssetListFilter.cs`:

```csharp
namespace MVVM.CoreEditor
{
    public enum AssetListFilter
    {
        All,
        Events,
        ReactiveVariables,
        Unused,
        PossibleDuplicates
    }
}
```

- [ ] **Step 3: Write MvvmExplorerWindow**

Create `Editor/Explorer/MvvmExplorerWindow.cs`:

```csharp
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class MvvmExplorerWindow : EditorWindow
    {
        private readonly MvvmAssetRepository _mvvmAssetRepository = new MvvmAssetRepository();

        private AssetListPanel _assetListPanel;
        private string _pendingSelectionGuid;

        [MenuItem("Tools/MVVM/Explorer")]
        public static void ShowWindow()
        {
            GetWindow<MvvmExplorerWindow>("MVVM Explorer");
        }

        public static void ShowWindowWithAsset(Object asset)
        {
            MvvmExplorerWindow window = GetWindow<MvvmExplorerWindow>("MVVM Explorer");
            window.SelectAsset(asset);
        }

        public void SelectAsset(Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            _pendingSelectionGuid = AssetDatabase.AssetPathToGUID(assetPath);

            _assetListPanel?.SelectAssetWithGuid(_pendingSelectionGuid);
        }

        private void CreateGUI()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarButton rebuildButton = new ToolbarButton(RebuildIndex);
            rebuildButton.text = "Rebuild Index";
            toolbar.Add(rebuildButton);

            _assetListPanel = new AssetListPanel();

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_assetListPanel.Root);

            LoadAssets();
        }

        private void RebuildIndex()
        {
            ReferenceIndexProvider.Rebuild(referenceIndex => ShowAssetsWithIndex(referenceIndex));
        }

        private void LoadAssets()
        {
            ReferenceIndexProvider.GetIndex(referenceIndex => ShowAssetsWithIndex(referenceIndex));
        }

        private void ShowAssetsWithIndex(ReferenceIndex referenceIndex)
        {
            _assetListPanel.SetAssets(_mvvmAssetRepository.GetAllAssets(), referenceIndex);

            if (string.IsNullOrEmpty(_pendingSelectionGuid))
                return;

            _assetListPanel.SelectAssetWithGuid(_pendingSelectionGuid);
        }
    }
}
```

- [ ] **Step 4: HANDOFF — ask the user to open the window and verify**

Ask the user to open `Tools > MVVM > Explorer` and report:

1. A progress bar appears on first open ("Indexing references…"), then finishes.
2. The list is populated with MVVM SO assets, sorted by name.
3. Each row shows a **reference count** on the right.
4. Typing in the search field filters the list.
5. Switching the filter to `Events` / `ReactiveVariables` / `Unused` / `PossibleDuplicates` changes what is listed.
6. Scrolling ~1000 rows is smooth (this proves virtualization is working).
7. Closing and reopening the window is **instant** (this proves the `Library/` cache is working).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer
git -C Packages/UP-MVVM commit -m "[A] Added mvvm explorer window and asset list panel"
```

---

### Task 11: ReferencesPanel

The tracing pane. Shows who references the selected asset, merging the index (disk) with the open-scene scan (unsaved).

**Files:**
- Create: `Editor/Explorer/ReferencesPanel.cs`
- Modify: `Editor/Explorer/MvvmExplorerWindow.cs`

**Interfaces:**
- Consumes: `MvvmAsset` (6), `ReferenceIndex` (3), `OpenSceneReferenceScanner` (9).
- Produces: `ReferencesPanel.Root` (`VisualElement`), `ReferencesPanel.ShowAsset(MvvmAsset asset, ReferenceIndex referenceIndex)`.

- [ ] **Step 1: Write ReferencesPanel**

Create `Editor/Explorer/ReferencesPanel.cs`:

```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class ReferencesPanel
    {
        private readonly VisualElement _root = new VisualElement();
        private readonly OpenSceneReferenceScanner _openSceneReferenceScanner = new OpenSceneReferenceScanner();

        public VisualElement Root => _root;

        public ReferencesPanel()
        {
            _root.style.flexGrow = 1;
        }

        public void ShowAsset(MvvmAsset asset, ReferenceIndex referenceIndex)
        {
            _root.Clear();

            _root.Add(CreateTitle(asset.Name));
            _root.Add(new Label(asset.TypeName));

            AddIndexedReferences(asset, referenceIndex);
            AddOpenSceneReferences(asset);
        }

        private void AddIndexedReferences(MvvmAsset asset, ReferenceIndex referenceIndex)
        {
            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(asset.Guid);

            _root.Add(CreateTitle($"References ({referencingPaths.Count})"));

            if (referencingPaths.Count == 0)
            {
                _root.Add(new Label("No references found."));
                return;
            }

            foreach (string referencingPath in referencingPaths)
                _root.Add(CreateAssetButton(referencingPath));
        }

        private void AddOpenSceneReferences(MvvmAsset asset)
        {
            Object targetAsset = AssetDatabase.LoadAssetAtPath<Object>(asset.Path);
            IReadOnlyList<Component> referencingComponents =
                _openSceneReferenceScanner.GetReferencingComponentsWithAsset(targetAsset);

            if (referencingComponents.Count == 0)
                return;

            _root.Add(CreateTitle($"In open scenes ({referencingComponents.Count})"));

            foreach (Component referencingComponent in referencingComponents)
                _root.Add(CreateComponentButton(referencingComponent));
        }

        private Button CreateAssetButton(string referencingPath)
        {
            Button button = new Button(() => SelectAssetWithPath(referencingPath));
            button.text = referencingPath;

            return button;
        }

        private Button CreateComponentButton(Component referencingComponent)
        {
            Button button = new Button(() => SelectGameObject(referencingComponent.gameObject));
            button.text = $"{referencingComponent.gameObject.name} ({referencingComponent.GetType().Name})";

            return button;
        }

        private void SelectAssetWithPath(string referencingPath)
        {
            Object referencedAsset = AssetDatabase.LoadMainAssetAtPath(referencingPath);

            Selection.activeObject = referencedAsset;
            EditorGUIUtility.PingObject(referencedAsset);
        }

        private void SelectGameObject(GameObject referencingGameObject)
        {
            Selection.activeGameObject = referencingGameObject;
            EditorGUIUtility.PingObject(referencingGameObject);
        }

        private Label CreateTitle(string title)
        {
            Label titleLabel = new Label(title);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginTop = 8;

            return titleLabel;
        }
    }
}
```

- [ ] **Step 2: Wire it into the window with a split view**

In `MvvmExplorerWindow.cs`, add the field:

```csharp
        private ReferencesPanel _referencesPanel;
        private ReferenceIndex _referenceIndex;
```

Replace `CreateGUI` with:

```csharp
        private void CreateGUI()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarButton rebuildButton = new ToolbarButton(RebuildIndex);
            rebuildButton.text = "Rebuild Index";
            toolbar.Add(rebuildButton);

            _assetListPanel = new AssetListPanel();
            _referencesPanel = new ReferencesPanel();
            _assetListPanel.OnAssetSelected = ShowAssetDetail;

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(_assetListPanel.Root);
            splitView.Add(_referencesPanel.Root);

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(splitView);

            LoadAssets();
        }

        private void ShowAssetDetail(MvvmAsset asset)
        {
            _referencesPanel.ShowAsset(asset, _referenceIndex);
        }
```

And in `ShowAssetsWithIndex`, store the index as the first line:

```csharp
        private void ShowAssetsWithIndex(ReferenceIndex referenceIndex)
        {
            _referenceIndex = referenceIndex;
            _assetListPanel.SetAssets(_mvvmAssetRepository.GetAllAssets(), referenceIndex);

            if (string.IsNullOrEmpty(_pendingSelectionGuid))
                return;

            _assetListPanel.SelectAssetWithGuid(_pendingSelectionGuid);
        }
```

- [ ] **Step 3: HANDOFF — ask the user to verify tracing**

Ask the user to open `Tools > MVVM > Explorer`, select an `EventViewModelSO` that is wired into a scene, and report:

1. The right pane lists referencing assets, and clicking one selects and pings it in the Project window.
2. **The critical check:** pick an event referenced by an **Installer ScriptableObject**. Confirm that SO appears. The old windows could never find this — if it shows, the whole index design is validated.
3. Drag the event into a fresh component in an open scene **without saving**, re-select it, and confirm it appears under **"In open scenes"**.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer/ReferencesPanel.cs Editor/Explorer/MvvmExplorerWindow.cs
git -C Packages/UP-MVVM commit -m "[A] Added references panel to the mvvm explorer"
```

---

### Task 12: EventRaiseRecorder + RuntimePanel

The runtime pane. Play mode only.

**Watching is opt-in.** Subscribing to all ~1000 events would call `GetEventViewModel()` on every one, force-loading every SO asset and instantiating 1000 view models. Only the **selected** asset is watched.

**Files:**
- Create: `Editor/Explorer/EventRaiseRecorder.cs`
- Create: `Editor/Explorer/RuntimePanel.cs`
- Modify: `Editor/Explorer/MvvmExplorerWindow.cs`

**Interfaces:**
- Consumes: `MvvmAsset` (6); `EventViewModelSO`, `BaseReactiveVariableSO` (runtime).
- Produces:
  - `EventRaiseRecorder.WatchAsset(EventViewModelSO eventViewModelSo)`, `.GetLog()` → `IReadOnlyList<string>`, `.Dispose()`.
  - `RuntimePanel.Root`, `.ShowAsset(MvvmAsset asset)`.

- [ ] **Step 1: Write EventRaiseRecorder**

Create `Editor/Explorer/EventRaiseRecorder.cs`:

```csharp
using System;
using System.Collections.Generic;
using MVVM.Core;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class EventRaiseRecorder : IDisposable
    {
        private const int MaximumLogEntries = 100;

        private readonly List<string> _log = new List<string>();

        private EventViewModelSO _watchedEventViewModelSo;
        private IEventViewModel _watchedEventViewModel;

        public Action OnLogChanged { get; set; }

        public IReadOnlyList<string> GetLog()
        {
            return _log;
        }

        public void WatchAsset(EventViewModelSO eventViewModelSo)
        {
            Dispose();

            _watchedEventViewModelSo = eventViewModelSo;
            _watchedEventViewModel = eventViewModelSo.GetEventViewModel();
            _watchedEventViewModel.OnEventRaised += RecordRaise;
        }

        public void Dispose()
        {
            if (ReferenceEquals(_watchedEventViewModel, null))
                return;

            _watchedEventViewModel.OnEventRaised -= RecordRaise;
            _watchedEventViewModel = null;
            _watchedEventViewModelSo = null;
            _log.Clear();
        }

        private void RecordRaise()
        {
            _log.Insert(0, $"[frame {Time.frameCount}] {_watchedEventViewModelSo.name} raised");

            if (_log.Count > MaximumLogEntries)
                _log.RemoveAt(_log.Count - 1);

            OnLogChanged?.Invoke();
        }
    }
}
```

The member names above are **verified**, not assumed — `IEventViewModel` is exactly:

```csharp
public interface IEventViewModel
{
    Action OnEventRaised { get; set; }
    void RaiseEvent();
}
```

- [ ] **Step 2: Write RuntimePanel**

Create `Editor/Explorer/RuntimePanel.cs`:

```csharp
using MVVM.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class RuntimePanel
    {
        private readonly VisualElement _root = new VisualElement();
        private readonly EventRaiseRecorder _eventRaiseRecorder = new EventRaiseRecorder();

        private MvvmAsset _shownAsset;

        public VisualElement Root => _root;

        public RuntimePanel()
        {
            _eventRaiseRecorder.OnLogChanged = RefreshLog;
            EditorApplication.playModeStateChanged += StopWatchingOnExit;
        }

        public void ShowAsset(MvvmAsset asset)
        {
            _shownAsset = asset;
            Refresh();
        }

        private void Refresh()
        {
            _root.Clear();

            if (ReferenceEquals(_shownAsset, null))
                return;

            if (!EditorApplication.isPlaying)
            {
                _root.Add(new Label("Enter play mode to inspect runtime state."));
                return;
            }

            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_shownAsset.Path);

            if (scriptableObject is EventViewModelSO eventViewModelSo)
            {
                ShowEvent(eventViewModelSo);
                return;
            }

            ShowReactiveVariable(scriptableObject as BaseReactiveVariableSO);
        }

        private void ShowEvent(EventViewModelSO eventViewModelSo)
        {
            _eventRaiseRecorder.WatchAsset(eventViewModelSo);

            Button raiseButton = new Button(() => eventViewModelSo.GetEventViewModel().RaiseEvent());
            raiseButton.text = "Raise Event";

            _root.Add(raiseButton);
            _root.Add(CreateLogContainer());
        }

        private void ShowReactiveVariable(BaseReactiveVariableSO baseReactiveVariableSo)
        {
            if (baseReactiveVariableSo == null)
                return;

            InspectorElement inspector = new InspectorElement(baseReactiveVariableSo);

            Button raiseButton = new Button(() => baseReactiveVariableSo.Raise());
            raiseButton.text = "Raise";

            _root.Add(inspector);
            _root.Add(raiseButton);
        }

        private VisualElement CreateLogContainer()
        {
            VisualElement logContainer = new VisualElement();
            logContainer.name = "log";

            foreach (string logEntry in _eventRaiseRecorder.GetLog())
                logContainer.Add(new Label(logEntry));

            return logContainer;
        }

        private void RefreshLog()
        {
            VisualElement logContainer = _root.Q<VisualElement>("log");

            if (logContainer == null)
                return;

            logContainer.Clear();

            foreach (string logEntry in _eventRaiseRecorder.GetLog())
                logContainer.Add(new Label(logEntry));
        }

        private void StopWatchingOnExit(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingPlayMode)
                return;

            _eventRaiseRecorder.Dispose();
            Refresh();
        }
    }
}
```

The live value display uses `InspectorElement`, which draws the SO's own inspector — including the `Debug > Value` field that `ReactiveVariableSO` already serializes in-editor. That is the field whose update path Task 1 fixed. There is no need to re-implement value rendering per type.

- [ ] **Step 3: Wire it into the window**

In `MvvmExplorerWindow.cs`, add:

```csharp
        private RuntimePanel _runtimePanel;
```

In `CreateGUI`, build the right-hand side as a vertical split, replacing the two `splitView.Add` lines:

```csharp
            _runtimePanel = new RuntimePanel();

            TwoPaneSplitView detailSplitView =
                new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Vertical);
            detailSplitView.Add(_referencesPanel.Root);
            detailSplitView.Add(_runtimePanel.Root);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(_assetListPanel.Root);
            splitView.Add(detailSplitView);
```

And extend `ShowAssetDetail`:

```csharp
        private void ShowAssetDetail(MvvmAsset asset)
        {
            _referencesPanel.ShowAsset(asset, _referenceIndex);
            _runtimePanel.ShowAsset(asset);
        }
```

- [ ] **Step 4: HANDOFF — ask the user to verify runtime behaviour**

Ask the user to report:

1. Out of play mode, selecting an asset shows "Enter play mode to inspect runtime state."
2. In play mode, selecting a **reactive variable** shows its live value, and it **updates as the value changes**. (This is the Task 1 fix paying off.)
3. In play mode, selecting an **event** shows a `Raise Event` button; clicking it appends a `[frame N] … raised` line to the log.
4. Gameplay raising that event also appends log lines.
5. Exiting play mode clears the log and shows the edit-mode message, with **no** Console errors.
6. **Enter and exit play mode three times** and confirm no duplicate log lines per raise (proves no handler accumulation).

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Explorer/EventRaiseRecorder.cs Editor/Explorer/RuntimePanel.cs Editor/Explorer/MvvmExplorerWindow.cs
git -C Packages/UP-MVVM commit -m "[A] Added runtime panel and event raise recorder"
```

---

### Task 13: MvvmAssetFactory + Create popup

The authoring pane. Warns about a similar existing name **as the user types** — the only moment duplication is cheap to prevent.

**Files:**
- Create: `Editor/Authoring/MvvmAssetFactory.cs`
- Create: `Editor/Authoring/CreateMvvmAssetPopup.cs`
- Modify: `Editor/Explorer/MvvmExplorerWindow.cs`

**Interfaces:**
- Consumes: `MvvmAsset`, `MvvmAssetRepository` (6), `DuplicateNameFinder` (5).
- Produces:
  - `MvvmAssetFactory.GetCreatableTypes()` → `IReadOnlyList<Type>`
  - `MvvmAssetFactory.Create(Type assetType, string assetName, string folderPath)` → `ScriptableObject`; **throws** on a missing folder or an exact name collision.

- [ ] **Step 1: Write MvvmAssetFactory**

Create `Editor/Authoring/MvvmAssetFactory.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class MvvmAssetFactory
    {
        public IReadOnlyList<Type> GetCreatableTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(IsCreatable)
                .OrderBy(assetType => assetType.Name)
                .ToList();
        }

        public ScriptableObject Create(Type assetType, string assetName, string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                throw new DirectoryNotFoundException($"The folder '{folderPath}' does not exist.");

            string assetPath = Path.Combine(folderPath, $"{assetName}.asset").Replace("\\", "/");

            if (!ReferenceEquals(AssetDatabase.LoadMainAssetAtPath(assetPath), null))
                throw new InvalidOperationException($"An asset already exists at '{assetPath}'.");

            ScriptableObject createdAsset = ScriptableObject.CreateInstance(assetType);

            AssetDatabase.CreateAsset(createdAsset, assetPath);
            AssetDatabase.SaveAssets();

            return createdAsset;
        }

        private bool IsCreatable(Type assetType)
        {
            if (assetType.IsAbstract)
                return false;

            if (assetType.IsGenericTypeDefinition)
                return false;

            if (typeof(EventViewModelSO).IsAssignableFrom(assetType))
                return true;

            return typeof(BaseReactiveVariableSO).IsAssignableFrom(assetType);
        }
    }
}
```

`TypeCache` is Unity's own indexed type lookup — far faster than reflecting over every loaded assembly.

- [ ] **Step 2: Write CreateMvvmAssetPopup**

Create `Editor/Authoring/CreateMvvmAssetPopup.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class CreateMvvmAssetPopup : EditorWindow
    {
        private const string DefaultFolderPath = "Assets";

        private readonly MvvmAssetFactory _mvvmAssetFactory = new MvvmAssetFactory();
        private readonly MvvmAssetRepository _mvvmAssetRepository = new MvvmAssetRepository();
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();

        private IReadOnlyList<Type> _creatableTypes;
        private PopupField<string> _typeField;
        private TextField _nameField;
        private TextField _folderField;
        private Label _warningLabel;

        public static void ShowPopup(Action onCreated)
        {
            CreateMvvmAssetPopup popup = CreateInstance<CreateMvvmAssetPopup>();
            popup.titleContent = new GUIContent("Create MVVM Asset");
            popup.OnCreated = onCreated;
            popup.ShowUtility();
        }

        public Action OnCreated { get; set; }

        private void CreateGUI()
        {
            _creatableTypes = _mvvmAssetFactory.GetCreatableTypes();

            List<string> typeNames = _creatableTypes.Select(creatableType => creatableType.Name).ToList();
            _typeField = new PopupField<string>("Type", typeNames, 0);

            _nameField = new TextField("Name");
            _nameField.RegisterValueChangedCallback(nameChange => RefreshWarning());

            _folderField = new TextField("Folder");
            _folderField.SetValueWithoutNotify(GetSelectedFolderPath());

            _warningLabel = new Label();
            _warningLabel.style.color = Color.yellow;

            Button createButton = new Button(CreateAsset);
            createButton.text = "Create";

            rootVisualElement.Add(_typeField);
            rootVisualElement.Add(_nameField);
            rootVisualElement.Add(_folderField);
            rootVisualElement.Add(_warningLabel);
            rootVisualElement.Add(createButton);
        }

        private void RefreshWarning()
        {
            _warningLabel.text = GetSimilarNameWarning();
        }

        private string GetSimilarNameWarning()
        {
            if (string.IsNullOrEmpty(_nameField.value))
                return string.Empty;

            List<string> names = _mvvmAssetRepository.GetAllAssets()
                .Select(asset => asset.Name)
                .ToList();

            names.Add(_nameField.value);

            foreach (IReadOnlyList<string> duplicateGroup in _duplicateNameFinder.GetDuplicateGroups(names))
            {
                if (!duplicateGroup.Contains(_nameField.value))
                    continue;

                IEnumerable<string> existingNames = duplicateGroup.Where(name => name != _nameField.value);

                return $"Similar asset already exists: {string.Join(", ", existingNames)}";
            }

            return string.Empty;
        }

        private void CreateAsset()
        {
            Type assetType = _creatableTypes[_typeField.index];
            ScriptableObject createdAsset = _mvvmAssetFactory.Create(assetType, _nameField.value, _folderField.value);

            Selection.activeObject = createdAsset;
            EditorGUIUtility.PingObject(createdAsset);

            OnCreated?.Invoke();
            Close();
        }

        private string GetSelectedFolderPath()
        {
            if (ReferenceEquals(Selection.activeObject, null))
                return DefaultFolderPath;

            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(selectedPath))
                return DefaultFolderPath;

            if (AssetDatabase.IsValidFolder(selectedPath))
                return selectedPath;

            return System.IO.Path.GetDirectoryName(selectedPath).Replace("\\", "/");
        }
    }
}
```

- [ ] **Step 3: Add the toolbar button**

In `MvvmExplorerWindow.CreateGUI`, add before the rebuild button:

```csharp
            ToolbarButton createButton = new ToolbarButton(ShowCreatePopup);
            createButton.text = "+ Create";
            toolbar.Add(createButton);
```

And add the method:

```csharp
        private void ShowCreatePopup()
        {
            CreateMvvmAssetPopup.ShowPopup(LoadAssets);
        }
```

- [ ] **Step 4: HANDOFF — ask the user to verify authoring**

Ask the user to report:

1. `+ Create` opens the popup with a type dropdown listing the concrete SO types (`EventViewModelSO`, `IntReactiveVariableSO`, `StringReactiveVariableSO`, …).
2. The folder defaults to the folder currently selected in the Project window.
3. Typing the name of an existing event (e.g. an existing `PlayerDied`) shows the yellow **"Similar asset already exists"** warning.
4. `Create` creates the asset, pings it, and it appears in the Explorer list.
5. Creating into a nonexistent folder shows a thrown error rather than silently failing.

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-MVVM add Editor/Authoring Editor/Explorer/MvvmExplorerWindow.cs
git -C Packages/UP-MVVM commit -m "[A] Added mvvm asset factory and create popup"
```

---

### Task 14: Delete the old searcher windows

Only now, once the Explorer covers everything they did — and more.

**Files:**
- Delete: `Editor/EventViewModel/EventViewModelReferencesEditorWindow.cs` (+ `.meta`)
- Delete: `Editor/ReactiveVariables/ReactiveVariableReferencesEditorWindow.cs` (+ `.meta`)
- Modify: `Editor/EventViewModel/EventViewModelEditor.cs:24-25,39-42`
- Modify: `Editor/EventViewModel/ReactiveVariableEditor.cs:8`

**Interfaces:**
- Consumes: `MvvmExplorerWindow.ShowWindowWithAsset(Object)` (10).
- Produces: nothing.

- [ ] **Step 1: Retarget the inspector button**

In `EventViewModelEditor.cs`, replace the button and its method:

```csharp
            if (GUILayout.Button("Open In MVVM Explorer"))
                MvvmExplorerWindow.ShowWindowWithAsset(eventViewModelSo);
```

and delete the now-unused `DrawSearchReferenceInTheScene` method entirely.

- [ ] **Step 2: Add the same button to ReactiveVariableEditor**

In `ReactiveVariableEditor.OnInspectorGUI`, after `DrawRaiseEventIfIsPlaying`:

```csharp
            if (GUILayout.Button("Open In MVVM Explorer"))
                MvvmExplorerWindow.ShowWindowWithAsset(reactiveVariableSo);
```

- [ ] **Step 3: Fix the ReactiveVariableEditor CustomEditor target**

`ReactiveVariableEditor` is declared `[CustomEditor(typeof(ReactiveVariableSO<>), true)]` — an **open generic type**. Unity's `CustomEditor` matching against an open generic is unreliable, which likely means this inspector has not been binding to the concrete subclasses at all.

Change it to target the non-generic base:

```csharp
    [CustomEditor(typeof(BaseReactiveVariableSO), true)]
    public class ReactiveVariableEditor : Editor
```

Also delete the unused `private List<Component> _allComponentReferences;` field from **both** inspector classes — it is dead in each.

**Verify this one specifically** in Step 5: it is a behaviour change, and if the old attribute *was* working, this must not regress it.

- [ ] **Step 4: Delete the two old window files**

```bash
git -C Packages/UP-MVVM rm Editor/EventViewModel/EventViewModelReferencesEditorWindow.cs Editor/EventViewModel/EventViewModelReferencesEditorWindow.cs.meta
git -C Packages/UP-MVVM rm Editor/ReactiveVariables/ReactiveVariableReferencesEditorWindow.cs Editor/ReactiveVariables/ReactiveVariableReferencesEditorWindow.cs.meta
```

- [ ] **Step 5: HANDOFF — ask the user to verify**

Ask the user to report:

1. `Tools > MVVM` now shows **only** `Explorer` — `Event Searcher` and `Reactive Searcher` are gone.
2. Selecting an `EventViewModelSO` shows an `Open In MVVM Explorer` button that opens the Explorer **with that asset selected**.
3. Selecting an `IntReactiveVariableSO` shows the **same** button, plus the `Raise` button in play mode. (This confirms the `CustomEditor` target fix.)
4. The Console is clean.

- [ ] **Step 6: Commit**

```bash
git -C Packages/UP-MVVM add Editor/EventViewModel Editor/ReactiveVariables
git -C Packages/UP-MVVM commit -m "[D] Deleted the event and reactive searcher windows replaced by the explorer"
```

---

### Task 15: Version bump and README

**Files:**
- Modify: `package.json:3`
- Modify: `README.md`

**Interfaces:**
- Consumes: nothing.
- Produces: nothing.

- [ ] **Step 1: Bump the version**

In `package.json`, change `"version": "1.30.2"` to `"version": "1.31.0"` — a minor bump: new feature, no breaking runtime change.

- [ ] **Step 2: Document the Explorer in the README**

Append to `README.md`:

```markdown
## MVVM Explorer

`Tools > MVVM > Explorer`

One window for the project's `EventViewModelSO` and `ReactiveVariableSO` assets:

- **Discovery** — searchable, filterable list of every MVVM asset with its reference count.
  Filter by `Unused` (nothing references it) or `PossibleDuplicates` (lexically near-identical names).
- **Tracing** — who references the selected asset, across scenes, prefabs, and other ScriptableObjects.
- **Runtime** — in play mode, the live value of a reactive variable, and a log of when an event was raised.
- **Authoring** — `+ Create` makes a new asset, warning as you type if a similar name already exists.

References are served from an index cached in `Library/`, built once and kept current as assets change.
If results ever look stale (after a `git pull` with the Editor closed, for instance), press **Rebuild Index**.

Known limits: the duplicate filter is lexical, so it catches `PlayerDied` / `OnPlayerDied` / `PlayersDied`
but not synonyms like `PlayerDeath`. `Unused` means "no reference found in any asset" — an SO loaded from
code rather than wired in the inspector will be reported as unused.
```

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-MVVM add package.json README.md
git -C Packages/UP-MVVM commit -m "[U] Updated package version and documented the mvvm explorer"
```

---

## Definition of Done

- [ ] All 20 EditMode tests pass in the Unity Test Runner (**confirmed by the user**, not assumed).
- [ ] `Tools > MVVM > Explorer` lists ~1000 assets, scrolls smoothly, and reopens instantly.
- [ ] The references pane finds a reference held by an **Installer ScriptableObject** (impossible in the old windows).
- [ ] Play mode shows live reactive values and an event raise log, surviving three enter/exit cycles cleanly.
- [ ] `+ Create` warns on a similar existing name before creating.
- [ ] The two old searcher windows are gone, and both inspectors open the Explorer instead.
- [ ] The Console is clean.
