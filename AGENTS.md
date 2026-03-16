# Repository Guidelines

## Project Structure & Module Organization
The playable code lives in `Project 1/`. Core gameplay systems are split by domain: `GameObjects/`, `Tiles/`, `Items/`, `Input/`, `UI/`, `Managers/`, and `Messaging/`. Art, shaders, and MonoGame content are under `Project 1/Content/`. The solution file is `Project 1.sln` at the repo root. Build outputs go to `Project 1/bin/` and `Project 1/obj/`; do not commit those.

## Build, Test, and Development Commands
- `dotnet build 'Project 1.sln' -v q`  
  Builds the solution and runs the MonoGame content pipeline.
- `dotnet build 'Project 1/Project 1.csproj' -v q`  
  Builds just the game project.
- `dotnet run --project 'Project 1/Project 1.csproj'`  
  Launches the game locally.

On Linux-based agents, content builds may fail if `mgcb` depends on Wine. If that happens, treat it as an environment issue unless content files were changed.

## Coding Style & Naming Conventions
Use 4-space indentation and keep files in ASCII unless a file already requires Unicode. Follow existing C# naming: `PascalCase` for types, methods, and public members; `camelCase` for locals and private fields when not using the existing `aParameter` style. Prefer small, domain-focused classes over large manager files. Keep state orchestration in routers/managers and domain behavior near the owning system.

## Testing Guidelines
There is no dedicated automated test project in this repository today. For code changes, at minimum:
- build the solution if the environment supports MGCB
- exercise the affected feature in-game
- note any unverified paths in your change summary

If you add tests later, place them in a separate sibling test project and name files after the class under test, for example `TileManagerTests.cs`.

## Commit & Pull Request Guidelines
Recent history uses short, informal messages. For new commits, use clear imperative summaries instead, for example `Refactor chat commands into router`. Keep each commit scoped to one concern. Pull requests should include:
- what changed
- why it changed
- any manual test coverage
- screenshots for UI/visual changes
- environment caveats such as MGCB/Wine issues

## Contributor Notes
Do not revert unrelated local changes. Prefer workspace-only edits, and verify file ownership before moving code across systems like `StateManager`, routers, and managers.
