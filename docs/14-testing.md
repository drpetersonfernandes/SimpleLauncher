# 14 — Testing

> The test project, conventions, coverage, and how to run the tests.
> Related: [ManualTests.md](../ManualTests.md) (manual checklist) · [02 — Projects & Solution](02-projects-and-solution.md)

## Project

- `SimpleLauncher.Tests` — xUnit (`net10.0-windows`), references `SimpleLauncher` (and transitively `SimpleLauncher.Core`); `InternalsVisibleTo` gives access to internal members.
- Frameworks: **xUnit**, **Moq 4.20.72**, Serilog `ILogger` mocks via `NoOpLogger`; Meziantou analyzer enabled.
- **150 test files** (~2000 tests). Parallelization is disabled (`AssemblyInfo.cs`: `CollectionBehavior(DisableTestParallelization = true)`) because several tests share static/WPF state.

## Test helpers (`SimpleLauncher.Tests\TestHelpers\`)

| Helper | Purpose |
|---|---|
| `NoOpLogger` | Serilog `ILogger` that discards everything |
| `NoOpMessageBoxLibraryService` | Stub for `IMessageBoxLibraryService` (large; all methods) |
| `NoOpCredentialProtector` | Plaintext credential protector for settings tests |
| `NoOpGetListOfFiles` / `NoOpResourceProvider` | Other no-op stubs |
| `ServiceProviderMock` | Installs a fake `IServiceProvider` where static access is required |
| `ProjectPathHelper` | Resolves repo-relative paths for tests (e.g. `parameters.md`) |
| `StaApartment` | Runs test actions on an STA thread (with optional WPF `Application`) for headless UI tests |

## What is covered (summary)

Unit tests cover the core logic layer: settings & system manager persistence, favorites, play history, game scanner core, file finder, search orchestrator, launch strategies (default, DOSBox, Commander Genius, CHD/CUE, PBP, XISO, ZIP), mount-strategy matching, Core emulator config-injection services, models/DTOs, path/URL/sanitizer/pagination/filter helpers, RetroAchievements manager/matcher/hasher, Steam VDF parser, update-check logic, API connectivity, converters' strategy classes — plus the batch added in 5.6.x: parameter resolver API service, `system.xml` writer + emulator XML helpers, game file watcher, loading overlay, UI reset, status bar, menu check-marks, credential protector (DPAPI), system-selection ViewModel, default-folder/temp/missing-file services.

**What is not covered** (windows, Views, ViewModels, live file operations, store scanners, gamepad/audio, RA API layer, per-emulator launch handlers) — see **[ManualTests.md](../ManualTests.md)** for the full manual checklist.

## Running the tests

```bash
# build + run everything
dotnet test SimpleLauncher.Tests/SimpleLauncher.Tests.csproj

# run a single class or feature
dotnet test SimpleLauncher.Tests/SimpleLauncher.Tests.csproj \
  --filter "FullyQualifiedName~GameFileWatcherServiceTests"

# run everything except the slow network/integration tests
dotnet test SimpleLauncher.Tests/SimpleLauncher.Tests.csproj \
  --filter "FullyQualifiedName!~IntegrationTests&FullyQualifiedName!~ApiConnectivity&FullyQualifiedName!~StatsApiConnection&FullyQualifiedName!~UpdateSimulation&FullyQualifiedName!~RetroAchievementsManager"
```

> ⚠ **Known slow test:** `UrlValidationTests.ParametersMdAllUrlsAreReachable` pings every URL in `parameters.md` over the network **without a timeout** — it can take 15+ minutes or hang when a URL is unreachable. Exclude it (or fix it with a `CancellationToken` timeout) for local runs.

## Writing new tests — conventions

- One test file per production class: `XxxTests.cs`; test class name matches the target.
- `using Xunit;` explicitly (no global using).
- Mock interfaces with Moq; use real temp directories for filesystem services (`Path.GetTempPath()` + GUID, cleanup in `Dispose`).
- WPF-touching tests run inside `StaApartment.Run/RunAsync` (see `LoadingOverlayServiceTests`, `MenuCheckMarkServiceTests`, `UiResetServiceTests`).
- Timing-based tests (watcher debounce) keep delays small and use `TaskCompletionSource` + `WaitAsync` timeouts.
- String assertions use `StringComparer.Ordinal` overloads (Meziantou MA0002/MA0074).

## Related docs

- [ManualTests.md](../ManualTests.md)
- [15 — Development](15-development.md)
- [13 — Logging & Debug](13-logging-and-debug.md)
