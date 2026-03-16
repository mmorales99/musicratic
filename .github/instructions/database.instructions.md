---
description: "Use when writing EF Core migrations, database configurations, or SQL scripts for Musicratic."
applyTo: ["src/**/Migrations/**", "src/**/EntityConfigurations/**"]
---

# Database Migration Standards

## Naming

- Tables: `snake_case`, plural (e.g., `queue_entries`).
- Columns: `snake_case` (e.g., `created_at`).
- Migrations: descriptive name (e.g., `AddVotingWindowToHubSettings`).

## Required

- Every table: `id` (UUID PK), `created_at`, `updated_at`.
- Hub-scoped tables: `tenant_id` (UUID NOT NULL, FK → hubs).
- Fluent API only — no Data Annotations.
- Both `Up()` and `Down()` methods must be implemented.

## PostgreSQL Types

- `uuid` for identifiers.
- `timestamptz` for all DateTime.
- `jsonb` for embedded objects (HubSettings).
- `citext` for case-insensitive strings.
