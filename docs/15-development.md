# 15 — Development

> Build, test, publish, versioning, localization, code style.
> Related: [02 — Projects & Solution](02-projects-and-solution.md) · [14 — Testing](14-testing.md)

## Prerequisites

- .NET SDK **10.0.x** (`global.json` pins `10.0.0`, roll-forward latestMajor, no prereleases).
- Windows (the projects target `net10.0-windows` and use WPF/DPAPI).

## Build & test

```bash
# build everything
dotnet build SimpleLauncher.sln

# build one project
dotnet build SimpleLauncher/SimpleLauncher.csproj

# run unit tests
dotnet test SimpleLauncher.Tests/SimpleLauncher.Tests.csproj
```

See [14 — Testing](14-testing.md) for test filters and the known slow network test.

## Publish (win-x64 / win-arm64)

```bash
dotnet publish SimpleLauncher/SimpleLauncher.csproj -c Release -r win-x64
dotnet publish SimpleLauncher/SimpleLauncher.csproj -c Release -r win-arm64
```

- `RuntimeIdentifiers` are `win-x64;win-arm64`; every bundled tool ships both variants (`X.exe` + `X_arm64.exe`) and is resolved per architecture at runtime (see [11 — Bundled Tools](11-bundled-tools.md)).
- Release zip naming for updates: `release_{version}_{rid}.zip` + `updater_{rid}.zip` (see [16 — Updater](16-updater.md)).
- A `publish-check\` folder with per-RID outputs is used locally to validate published payloads.

### Publish the Avalonia app (multi-targeted)

The Avalonia app targets both `net10.0` (Linux) and `net10.0-windows` (Windows), so the
target framework **must** be specified when publishing:

```bash
dotnet publish SimpleLauncher.Avalonia/SimpleLauncher.Avalonia.csproj -c Release -f net10.0-windows -r win-x64
dotnet publish SimpleLauncher.Avalonia/SimpleLauncher.Avalonia.csproj -c Release -f net10.0-windows -r win-arm64
dotnet publish SimpleLauncher.Avalonia/SimpleLauncher.Avalonia.csproj -c Release -f net10.0 -r linux-x64
dotnet publish SimpleLauncher.Avalonia/SimpleLauncher.Avalonia.csproj -c Release -f net10.0 -r linux-arm64
```

- The `net10.0` TFM is **Linux-only** (audio uses libsndfile/`SoundFileReader`). Publishing it
  with a Windows RID (`-f net10.0 -r win-x64`) is rejected by a build guard: the `WINDOWS`
  symbol would not be defined, so `PlaySoundEffects` would take the Linux path and crash on
  Windows (`DllNotFoundException: libsndfile`, no Windows native binary is shipped).
- The Windows publish uses Media Foundation + WaveOut, so `libsndfile` is not needed there.

## Versioning

Version `5.6.0` must stay in sync across:

- `SimpleLauncher\SimpleLauncher.csproj` (`AssemblyVersion`, `FileVersion`, `Version`)
- `SimpleLauncher.Core\SimpleLauncher.Core.csproj` (same three)
- `SimpleLauncher.Tests\SimpleLauncher.Tests.csproj`
- `SimpleLauncher\app.manifest` (`assemblyIdentity version`)
- `SimpleLauncher.Updater\version.txt` (`release5.6.0`)

`VersionConsistencyTests` enforces this in CI/local runs. Bump all of them together.

## Localization

- 18 languages as WPF resource dictionaries: `SimpleLauncher\resources\strings.{code}.xaml` (ar, bn, de, en, es, fr, hi, id, it, ja, ko, nl, pt-br, ru, tr, ur, vi, zh-hans).
- `SimpleLauncher.ResourceTranslator` project assists adding/validating translations.
- Unit tests guard against common translation issues: missing keys in other languages, duplicate/mismatched resource keys, empty values, key-count mismatches (`DetectMissingResourceStringsTests` family).
- Add a new language: create `strings.{code}.xaml`, register it in `App.ChangeLanguage`/`LanguageMenuService`, and update the resource-key tests if needed.

## Static analysis & code style

- **Meziantou.Analyzer 3.0.139** and **Microsoft.CodeAnalysis.NetAnalyzers 10.0.302** (both `PrivateAssets`).
- `Nullable` enabled everywhere; `LangVersion 14`; implicit usings + global `using System.IO; using System.Net.Http; using Serilog;`.
- `NoWarn` in app: `NU1903;CS0436`.
- Tests must satisfy the analyzers (e.g. `StringComparison` overloads on string assertions).
- Conventions observed in the codebase: services take Serilog `ILogger`; UI services use the host-interface pattern (`Initialize(host)`) instead of receiving windows; ViewModels use CommunityToolkit.Mvvm.

## Publishing the docs (Pages + wiki)

The `docs\` folder is published in two places:

### GitHub Pages (automatic, no credentials needed)

The site is served from `/docs` via **docsify** (client-side rendering, no build step):

1. Enable once in repo settings: **Settings → Pages → Deploy from a branch → branch `master`, folder `/docs`**.
2. Every push to `docs/**` rebuilds the site automatically.
3. URL: `https://drpetersonfernandes.github.io/SimpleLauncher/`.

`docs/index.html` (docsify loader), `docs/_sidebar.md` (TOC) and `docs/.nojekyll` are the site assets. `docs/parameters.md` and `docs/manual-tests.md` are **copies** kept for the site and wiki:

- `docs/parameters.md` ← `SimpleLauncher/parameters.md` — refresh it whenever the canonical file changes.
- `docs/manual-tests.md` ← `ManualTests.md` (repo root) — refresh likewise.

### GitHub Wiki (local sync script)

The wiki is a separate git repo (`SimpleLauncher.wiki.git`); the default `GITHUB_TOKEN` cannot push to it, so syncing runs locally (or in CI with a PAT):

```bash
python scripts/sync-wiki.py --dry-run   # preview
python scripts/sync-wiki.py             # clone/pull, rewrite, commit, push
```

The script maps `docs/README.md` → `Home`, copies all `docs/NN-*.md` as pages, and **protects the `parameters` page** (`https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters`) — the app opens this URL (`EditSystemWindow.xaml.cs`, config key `WikiParametersUrl`), so it is never deleted and is refreshed from `docs/parameters.md`. Stale pages are deleted, `_Sidebar.md` is regenerated, and markdown links are rewritten to the flat wiki namespace.

For CI automation (optional): add a workflow that runs the script with a `WIKI_PAT` secret (classic PAT, `repo` scope) on `docs/**` pushes.

## Release workflow (from git history & What's New)

1. Implement features/fixes; keep `WhatsNew.md` updated with a release section.
2. Bump version in the five places above.
3. Run the full test suite (minus the slow URL test).
4. Publish both RIDs; package `release_{version}_{rid}.zip` + `updater_{rid}.zip`; create a GitHub release.
5. The in-app updater and silent update check use the GitHub `releases/latest` API.

## Related docs

- [02 — Projects & Solution](02-projects-and-solution.md)
- [14 — Testing](14-testing.md)
- [16 — Updater](16-updater.md)
- [17 — Release Notes](17-release-notes.md)
