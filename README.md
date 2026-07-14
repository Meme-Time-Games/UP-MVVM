# MVVM

A reactive UI framework for Unity using ScriptableObject-based state and events.

## MVVM Explorer

`Tools > MVVM > Explorer`

One window for the project's `EventViewModelSO` and `ReactiveVariableSO` assets:

- **Discovery** — searchable, filterable list of every MVVM asset grouped by feature, with reference count per row.
  Groups derive from each type's `[CreateAssetMenu]` path and fall back to the asset's folder.
  Group headers collapse and expand. Filter by `Unused` (nothing references it) or `PossibleDuplicates` (lexically near-identical names).
- **Tracing** — who references the selected asset, across scenes, prefabs, and other ScriptableObjects.
- **Runtime** — in play mode, the live value of a reactive variable, and a log of when an event was raised.
- **Authoring** — `+ Create` asks for the kind (Event or Reactive Variable), offers a searchable hierarchical type picker, auto-suffixes the name with the type (`PlayerHealth` → `PlayerHealthIntReactiveVariableSO`), and lets you browse for a target folder.
- **Selection** — double-clicking a row selects and pings the asset in the Project window.

References are served from an index cached in `Library/`, built once and kept current as assets change.
If results ever look stale (after a `git pull` with the Editor closed, for instance), press **Rebuild Index**.

Known limits:
- The duplicate filter is lexical, so it catches `PlayerDied` / `OnPlayerDied` / `PlayersDied` but not synonyms like `PlayerDeath`.
- `Unused` means "no reference found in any asset" — an SO loaded from code rather than wired in the inspector will be reported as unused.
- Events are always grouped by folder, never by `[CreateAssetMenu]`. Every event asset in the project shares the single `EventViewModelSO` type, so its menu path carries no feature information — grouping events by it would put every event in one group.
