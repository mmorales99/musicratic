---
description: "Scaffold a new backend module with all 4 layers (Domain, Application, Infrastructure, Api) plus test project"
agent: "backend-architect"
---

Create a new backend module called `{{moduleName}}` with the full 4-layer structure:

1. `src/Modules/{{moduleName}}/Musicratic.{{moduleName}}.Domain/` — Base entity, value objects, domain events
2. `src/Modules/{{moduleName}}/Musicratic.{{moduleName}}.Application/` — Commands, queries, handlers, validators
3. `src/Modules/{{moduleName}}/Musicratic.{{moduleName}}.Infrastructure/` — EF Core configs, repositories, Dapr integration
4. `src/Modules/{{moduleName}}/Musicratic.{{moduleName}}.Api/` — gRPC service stubs, endpoint definitions
5. `tests/Musicratic.{{moduleName}}.Tests/` — xUnit test project

Each project must include:

- `.csproj` with correct project references
- `DependencyInjection.cs` for service registration
- Proper namespace matching folder structure

Reference the domain model in [docs/03-domain-model.md](docs/03-domain-model.md) for entity definitions.
Add the new projects to `Musicratic.sln`.
