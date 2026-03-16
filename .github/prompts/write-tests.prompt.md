---
description: "Write comprehensive tests for a specific module or feature with 100% coverage"
agent: "testing"
---

Write tests for: **{{target}}**

## Process

1. Read the source code for the target module/feature.
2. Read the relevant spec in `/docs/` for business rules.
3. List all testable scenarios:
   - Happy path for each public method
   - Edge cases (null, empty, boundary values)
   - Error cases (invalid input, unauthorized access)
   - Multi-tenant isolation (can't access other hub's data)
   - Concurrency scenarios where applicable
4. Write tests following the project conventions:
   - **Backend**: xUnit + Moq + FluentAssertions. Naming: `{Method}_Should{Behavior}_When{Condition}`
   - **Angular**: Playwright component + E2E tests
   - **Flutter**: flutter_test widget + Playwright E2E tests
5. Verify all branches are covered.

## Rules

- Reference spec documents in test comments for business rule tests.
- Mock all external dependencies.
- Tests must be independent — no shared mutable state.
- Target 100% coverage.
