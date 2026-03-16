---
description: "Implement a feature using parallel agent waves across backend, web, and mobile"
agent: "agent"
---

Implement the feature described below using **parallel agent waves**.

## Feature: {{featureDescription}}

## Parallel Execution Plan

```
Wave 1 — PARALLEL (if new entities/structure needed)
├── Chat 1 → @backend-module   "Implement {{featureDescription}} domain + application + infra + api"
├── Chat 2 → @database         "Create EF configs and migration for {{featureDescription}}"
└── (optional) @backend-architect   if new module/project needed

     ⏳ Wait for backend to define API contracts

Wave 2 — PARALLEL (frontend can start once API shape is known)
├── Chat 3 → @angular-web      "Build {{featureDescription}} in Angular with XState machine"
└── Chat 4 → @flutter-mobile   "Build {{featureDescription}} in Flutter with Bloc"

     ⏳ Wait for all code to exist

Wave 3
└── Chat 5 → @testing          "Write tests for {{featureDescription}} across all layers"
```

### Rules

- Follow domain model in [docs/03-domain-model.md](docs/03-domain-model.md).
- Enforce business rules from the relevant spec doc.
- Each agent writes ONLY in its owned directories — no file conflicts.
- Max ~300 lines per file. 100% test coverage.
