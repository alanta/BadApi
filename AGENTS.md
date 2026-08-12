# AGENTS.md

## Repository purpose
This repo is a secure-coding training API with intentionally vulnerable endpoints used to teach API security issues and their remediations. The scenarios are exposed through the HTTP files under `BadApi/*/*.http` and implemented in the endpoint registration code under `BadApi/*/Endpoints.cs`.

## Operating rules
- Use the .NET 10 SDK pinned by `global.json`.
- Use the `.slnx` solution file as the canonical solution entry point.
- Keep the training scenarios intact unless the task explicitly asks for a security fix or refactor.
- Prefer minimal, scenario-preserving changes; do not “fix” vulnerabilities unless the user asks for a remediation task.
- Do not remove scenario files or change route names unless needed for the requested work.

## Baseline commands
These are the commands verified in this repo:

```bash
dotnet --version
# should resolve to 10.0.302 via global.json

dotnet build BadApi.slnx

dotnet test BadApi.slnx --no-restore

# AppHost runtime smoke test
DOTNET_USE_POLLING_FILE_WATCHER=1 dotnet run --project AppHost/AppHost.csproj --no-build
```

## Important environment note
If the environment is under a strict inotify quota, the test host can fail with:

- `The configured user limit (128) on the number of inotify instances has been reached`

If that happens, use:

```bash
DOTNET_USE_POLLING_FILE_WATCHER=1 dotnet test BadApi.slnx --no-restore
```

This environment quirk was observed here; the default `dotnet test` path works again after the environment change, but the polling workaround remains a useful fallback for constrained containers or dev environments.

## Repo structure
- `AppHost/` - Aspire host for local orchestration
- `BadApi/` - vulnerable API and scenario endpoints
- `BadApi.Tests/` - integration tests for API behavior
- `DDoS/` - load-testing project
- `ServiceDefaults/` - Aspire shared defaults and telemetry configuration

## Dependency and upgrade workflow
- Use `global.json` as the single SDK source of truth.
- Use `dotnet outdated BadApi.slnx --pre-release Never` to check direct package drift.
- Use `dotnet outdated BadApi.slnx --pre-release Never --upgrade` to update direct references.
- Keep Aspire in sync with the AppHost SDK and package version sharing the same release line.
- Prefer stable packages only unless a task explicitly requires prerelease testing.

## Scenario validation guidance
Use the `.http` files as the student-facing checks for the intentionally vulnerable and fixed examples:
- `BadApi/Account/Account.http`
- `BadApi/BrokenAccessControl/BrokenAccessControl.http`
- `BadApi/DDoS/ping.http`
- `BadApi/OverPosting/OverPosting.http`
- `BadApi/SqlInjection/SqlInjection.http`
- `BadApi/XXE/XXE.http`

When editing security behaviors, validate the relevant request files or their equivalent `curl`/HTTP calls, then rerun the focused test suite or full `dotnet test BadApi.slnx`.

## Preferred workflow for agents
1. Read the specific endpoint and relevant `*.http` file before making a change.
2. Keep the existing scenario semantics unless the task explicitly requires modifying them.
3. Validate with the smallest necessary command: targeted project/build first, then full solution tests if the change affects shared infrastructure.
4. Prefer repo-native commands and the pinned .NET 10 SDK; avoid ad hoc environment assumptions.

## Notes for future upgrades
- The canonical solution format is `BadApi.slnx`, not the legacy `.sln` format.
- AppHost uses Aspire 13.4.6 with the .NET 10 SDK.
- This repo is intentionally insecure by design; do not “clean up” vulnerabilities unless explicitly requested.
