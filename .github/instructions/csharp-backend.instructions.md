---
description: "Use when writing C# backend code for Musicratic: entities, services, repositories, controllers, handlers, Dapr integration, EF Core configurations, or any .NET module code."
applyTo: "src/**/*.cs"
---

# C# Backend Standards

## Architecture

- 4-layer modules: Domain → Application → Infrastructure → Api.
- Domain has zero external dependencies.
- Modules communicate only via Dapr (pub/sub or service invocation).

## Patterns

- Entities: private setters, factory methods, domain validation.
- CQRS: commands/queries handled by MediatR (or similar).
- Entity mapping: Entity → DataDto → ApiDto (never expose domain entities to API).
- Error handling: domain exceptions → Problem Details (RFC 9457).
- Async: DON'T USE `Async` suffix (unless necessary), always pass `CancellationToken`.
- Config: DB → appsettings → compile-time constant fallback.

## EF Core

- Fluent API only (no Data Annotations).
- Global query filters for `tenant_id` on hub-scoped entities.
- `timestamptz` for all DateTime columns.
- `created_at` and `updated_at` on every table.

## Conventions

- `PascalCase` for files, classes, methods, properties.
- Max ~300 lines per file, 1 class per file.
- Explicit return types on all public methods.
- Constructor injection for dependencies.
- `DependencyInjection.cs` in each project for service registration.

## Logging

- Serilog structured logging.
- Include OpenTelemetry TraceId.
- Use `LogInformation`, `LogWarning`, `LogError` — never `Console.WriteLine`.

## Testing

- xUnit + Moq + FluentAssertions.
- 100% coverage enforced.
- Test naming: `{Method}_Should{Behavior}_When{Condition}`.
