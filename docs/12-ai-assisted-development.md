# 12 — AI-Assisted Development Guidelines

## Philosophy

Musicratic is an **AI-native project**. This means AI coding assistants are not an afterthought — they're a first-class development tool. Every convention, every file structure, every naming decision should optimize for AI readability and generation accuracy.

> The goal is not to replace the developer but to **amplify** them: a single developer + AI should be as productive as a small team on well-defined tasks.

---

## Core Principles

### 1. Documentation Is the Prompt

The `/docs/` folder is not just documentation — it's the **system prompt for AI agents**. When you ask an AI to implement a feature, point it at the relevant spec document. The more precise the spec, the better the generated code.

**Rule**: Before implementing any feature, ensure its spec document is complete and up-to-date. If the spec is ambiguous, clarify it *in the document* before writing code.

### 2. Small, Focused Files

AI models have context windows. Large files degrade generation quality.

| Guideline | Target |
|---|---|
| Max lines per file | ~300 lines |
| Max classes per file | 1 (exceptionally 2 if tightly coupled) |
| Max methods per class | ~10 |
| Max parameters per method | ~5 |

When a file grows beyond 300 lines, it's a signal to extract.

### 3. Explicit Over Implicit

AI models struggle with magic. Prefer explicit patterns:

- **Explicit imports** over barrel files.
- **Explicit type annotations** on public APIs.
- **Explicit error handling** over silent failures.
- **Named parameters** over positional parameters for 3+ args.
- **Enum values** over string constants.

### 4. Consistent Naming Conventions

Consistency is an AI's best friend. Deviation costs context.

| Concept | Convention | Example |
|---|---|---|
| Files (C#) | `PascalCase.cs` | `HubRepository.cs` |
| Files (Angular/TS) | `kebab-case.ts` | `hub-repository.service.ts` |
| Files (Dart) | `snake_case.dart` | `hub_repository.dart` |
| Classes (C#, TS, Dart) | `PascalCase` | `HubRepository` |
| Methods (C#) | `PascalCase` | `GetActiveHubs()` |
| Methods (TS, Dart) | `camelCase` | `getActiveHubs()` |
| Variables | `camelCase` | `currentQueue` |
| Constants (C#) | `PascalCase` | `MaxQueueSize` |
| Constants (TS, Dart) | `camelCase` | `maxQueueSize` |
| Enums (C#) | `PascalCase.Value` | `HubType.Venue` |
| Enums (TS, Dart) | `PascalCase.camelCase` | `HubType.venue` |
| Database tables | `snake_case` (plural) | `hub_members` |
| Database columns | `snake_case` | `created_at` |
| API routes | `kebab-case` | `/hubs/{id}/queue-entries` |
| Event names | `PascalCase` | `QueueUpdated` |
| Dapr topics | `{env}_{feature}_{action}` | `prod_voting_vote-cast` |
| Error types | `kebab-case` | `insufficient-coins` |
| gRPC services | `PascalCase` | `PlaybackService.ProposeTrack` |
| JSON-RPC methods | `{module}.{action}` | `voting.cast_vote` |
| GraphQL queries | `camelCase` | `query { activeHubs { ... } }` |
| Env vars (config) | `MUSICRATIC_{NAME}` | `MUSICRATIC_DB_HOST` |
| Env vars (secrets) | `MUSICRATIC_SECRET_{NAME}` | `MUSICRATIC_SECRET_STRIPE_KEY` |
| Roles | `snake_case` | `list_owner`, `hub_manager` |

### 5. Self-Documenting Code Over Comments

Good names eliminate the need for comments.

```csharp
// BAD
// Check if the user can skip
if (role == "owner" && voteType == "down") { ... }

// GOOD
if (isListOwner && votedDown)
{
    await SkipTrackImmediatelyAsync(entry);
}
```

Comments are warranted for:
- **Business rules** that aren't obvious from code (e.g., "50% refund on skip, per spec 05-voting-and-playback.md").
- **Workarounds** with links to issues.
- **TODO items** with clear scope.

---

## AI-Optimized Workflow

### Development Loop

```
1. Pick a task from the roadmap
       │
       ▼
2. Read the relevant spec document(s)
       │
       ▼
3. Break the task into atomic units:
   ├── Entity / model
   ├── Repository interface
   ├── Repository implementation
   ├── Use case(s)
   ├── Provider(s)
   ├── Screen / widget
   └── Tests
       │
       ▼
4. For each atomic unit:
   a. Describe what you need to the AI (reference the spec)
   b. Review generated code against the spec
   c. Integrate, run tests, iterate
       │
       ▼
5. Mark task complete in the roadmap
```

### Effective AI Prompting Patterns

#### Pattern: Spec-Driven Generation

```
"Implement the VotingService class per the rules defined in
docs/05-voting-and-playback.md. It should expose:
 - castVote(userId, queueEntryId, value)
 - getTally(queueEntryId)
 - shouldSkip(queueEntryId) → bool
Use the domain entities from docs/03-domain-model.md."
```

#### Pattern: Test-First Generation

```
"Write unit tests for the track pricing engine. The rules are:
 - Base cost = floor(duration_seconds / 60) coins
 - Hotness multiplier per docs/06-monetization.md
 - Final cost = floor(base * multiplier)
Cover edge cases: 59-second track, exactly 2-minute track,
viral hotness level."
```

#### Pattern: Scaffold + Fill

```
"Create the file structure for the 'voting' feature following
the Clean Architecture pattern defined in
docs/10-platform-and-tech-stack.md. Generate stub files with
TODO comments for each class and method."
```

Then, fill each stub individually with focused prompts.

---

## Instruction Files for AI Agents

### `.github/copilot-instructions.md`

A project-level instruction file that AI assistants (GitHub Copilot, Cursor, etc.) load automatically:

```markdown
# Musicratic — AI Coding Instructions

You are working on Musicratic, a collaborative music playback platform.

## Project Structure
- `src/` — C# / .NET backend (modular monolith)
- `src/Modules/` — Backend modules (Auth, Hub, Playback, Voting, Economy, etc.)
- `src/BFF/` — Stateless BFF layers (Web, Mobile)
- `src/Shared/` — Shared kernel, contracts, infrastructure
- `web/` — Angular web client
- `mobile/` — Flutter mobile client
- `docs/` — Specification documents (READ THESE FIRST)
- `dapr/` — Dapr component configuration

## Rules
1. Backend follows modular monolith: each module has Domain, Application, Infrastructure, Api layers.
2. Modules communicate only via Dapr pub/sub (events) or Dapr service invocation (sync).
3. Angular uses XState state machines for state management. All state is client-side.
4. Flutter uses bloc/freezed state machines. Mirrors Angular patterns.
5. BFF is stateless — no session state, no sticky sessions. Exposes JSON-RPC (commands), REST (resources), GraphQL (search). Communicates with backend via gRPC.
6. All public APIs must have explicit return types.
7. Max ~300 lines per file. Extract if larger.
8. Business rules come from `/docs/`. When in doubt, read the spec.
9. Write tests for every feature. **100% coverage** is enforced in CI (backend, web, mobile).
10. Tenant scoping: tenants isolate hub sessions and current activities only.
11. WebSocket events follow the protocol in docs/10-platform-and-tech-stack.md.
12. Error handling: use Problem Details (RFC 9457) for all errors, including inner exceptions.
13. Logging: use Serilog structured logging. Include TraceId from OpenTelemetry in all log entries.
14. All REST collection responses use the standard envelope: `{ success, total_items_in_response, has_more_items, items[], audit }`.
15. Dapr topics: `{environment}_{feature}_{action}` format. Tenant ID in message metadata, not topic name.
16. Configuration: appsettings for hot-swap, env vars (`MUSICRATIC_*`) for startup, Podman Secrets / AES-256 for secrets.
17. All DB values must have a fallback (DB → appsettings → compile-time constant). Missing data never crashes.
18. 5 roles (accumulative): anonymous, visitor, user, list_owner, hub_manager. See docs/07.
19. Auth uses JWE (encrypted) access tokens + opaque refresh tokens. Role resolved per-request, not embedded in token.
20. Only web/mobile app clients can access action endpoints. Validate origin/attestation.
```

### Feature-Level Instructions

Each feature directory may contain a `.instructions.md` file scoping AI behavior for that feature:

```
features/voting/.instructions.md
→ "When generating voting code, enforce the skip rules from
   docs/05-voting-and-playback.md. Owner downvote = instant skip.
   65% threshold only applies within the first 60 seconds."
```

---

## Code Review with AI

### Pre-Commit Checks

Before committing, use AI to review:

1. **Spec compliance**: "Does this implementation match docs/05-voting-and-playback.md?"
2. **Edge cases**: "What happens if no votes are cast? What if only 1 user is attached?"
3. **Security**: "Does this endpoint validate the user's role before executing?"
4. **Performance**: "Will this query scale with 1000 concurrent hub users?"

### Automated Code Quality

| Check | Tool | When |
|---|---|---|
| **Backend formatting** | `dotnet format` | Pre-commit |
| **Backend analysis** | `dotnet build /warnaserror` | Pre-commit + CI |
| **Angular linting** | `ng lint` | Pre-commit |
| **Angular formatting** | Prettier | Pre-commit |
| **Dart formatting** | `dart format` | Pre-commit |
| **Dart analysis** | `dart analyze` | Pre-commit + CI |
| **Backend tests** | `dotnet test` (xUnit + Moq, 100% coverage) | CI |
| **Angular unit tests** | Playwright component tests (100% coverage) | CI |
| **Angular E2E tests** | Playwright E2E (100% coverage) | CI |
| **Flutter tests** | `flutter test --coverage` + Playwright E2E (100% coverage) | CI |
| **Static analysis** | SonarQube (Community Edition, self-hosted) | CI — quality gate blocks merge |
| **Dependency audit** | `dotnet list package --outdated` / `npm outdated` / `dart pub outdated` | Weekly |

### `.editorconfig` (Shared across all projects)

```ini
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{ts,js,json,html,css,scss}]
indent_size = 2

[*.dart]
indent_size = 2
```

### `analysis_options.yaml` (Flutter — Strict)

```yaml
include: package:flutter_lints/flutter.yaml

linter:
  rules:
    - always_declare_return_types
    - always_use_package_imports
    - avoid_dynamic_calls
    - avoid_print
    - prefer_const_constructors
    - prefer_final_locals
    - require_trailing_commas
    - sort_constructors_first
    - unawaited_futures
    - unnecessary_lambdas

analyzer:
  errors:
    missing_return: error
    dead_code: warning
  exclude:
    - "**/*.g.dart"
    - "**/*.freezed.dart"
```

---

## Repository Conventions

### Commit Messages

Follow Conventional Commits:

```
feat(voting): implement 65% downvote auto-skip rule
fix(economy): round coin refund down instead of up
docs(hub): add shuffle weight formula to hub system spec
test(voting): add owner instant-skip edge cases
chore(ci): add desktop build targets to GitHub Actions
```

### Branch Strategy

```
main ← production-ready
  └── develop ← integration branch
        ├── feat/voting-skip-rules
        ├── feat/hub-qr-generation
        ├── fix/refund-rounding
        └── docs/mood-system-spec
```

### PR Templates

```markdown
## What
[Brief description]

## Spec Reference
[Link to relevant doc(s) in /docs/]

## Changes
- [ ] Feature implementation
- [ ] Tests added/updated
- [ ] Spec doc updated (if behavior changed)

## AI-Assisted
- [ ] Generated with AI assistance
- [ ] Manually reviewed against spec
- [ ] Edge cases verified
```

---

## Knowledge Capture

### When to Update Docs

| Trigger | Action |
|---|---|
| Implementing a feature reveals a spec gap | Update the spec document immediately |
| A business rule proves incorrect during testing | Update the spec, then fix the code |
| A new edge case is discovered | Add it to the spec's "Edge cases" section |
| Architecture decision changes | Update architecture doc + create ADR |

### Architecture Decision Records (ADRs)

For significant technical decisions, create an ADR in `docs/adr/`:

```
docs/adr/
├── 001-modular-monolith-over-microservices.md
├── 002-dapr-for-service-abstraction.md
├── 003-authentik-over-custom-auth.md
├── 004-memcached-over-redis.md
├── 005-podman-over-docker.md
├── 006-xstate-for-angular-state.md
└── ...
```

Format:
```markdown
# ADR-001: Use Serverpod over Shelf

## Status: Accepted
## Date: YYYY-MM-DD

## Context
[Why this decision was needed]

## Decision
[What was decided]

## Consequences
[Trade-offs accepted]
```

---

## Summary: The AI-Amplified Developer

| Principle | Practice |
|---|---|
| Docs are prompts | Write specs before code |
| Small files | ~300 line max, 1 class per file |
| Explicit code | Types, names, enums — no magic |
| Consistent conventions | Same patterns everywhere |
| Test-driven | AI generates tests from spec |
| Review with AI | Spec compliance, edge cases, security |
| Capture knowledge | Update docs when reality differs from spec |
