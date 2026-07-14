# MVVM

A reactive UI framework for Unity using ScriptableObject-based state and events.

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
