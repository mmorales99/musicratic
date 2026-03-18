# Testing & Quality — Backlog

## Project Summary

| Metric                | Value  |
| --------------------- | ------ |
| Total tasks           | 10     |
| Done                  | 10     |
| Remaining             | 0      |
| Est. premium requests | ~25    |
| Est. tokens           | ~430 K |

> Each module backlog includes inline unit tests as part of "Definition of Done" for every task.
> This backlog covers **cross-cutting** test infrastructure and integration/E2E tests.

## Phase 1A — Test Infrastructure

| ID       | Task                                                                            | Size | PRs | Tokens | Phase | Deps    | Agent   | Status  |
| -------- | ------------------------------------------------------------------------------- | ---- | --- | ------ | ----- | ------- | ------- | ------- |
| TEST-001 | Playwright E2E infrastructure for Angular (config, fixtures, auth helpers)      | M    | 3   | 50K    | 1A    | WEB-001 | testing | ✅ Done |
| TEST-002 | Playwright E2E infrastructure for Flutter web (config, fixtures)                | M    | 3   | 50K    | 1A    | MOB-001 | testing | ✅ Done |
| TEST-003 | Shared test utilities (entity builders, fake repositories, test data factories) | M    | 2   | 40K    | 1A    | —       | testing | ✅ Done |
| TEST-004 | CustomWebApplicationFactory enhancement (per-module SQLite, seeded test data)   | M    | 2   | 40K    | 1A    | —       | testing | ✅ Done |

## Phase 1A–1C — Integration Tests

| ID       | Task                                                                                    | Size | PRs | Tokens | Phase | Deps               | Agent   | Status  |
| -------- | --------------------------------------------------------------------------------------- | ---- | --- | ------ | ----- | ------------------ | ------- | ------- |
| TEST-005 | Integration test: Auth → Hub attachment flow (register → create hub → scan QR → attach) | L    | 4   | 70K    | 1A    | HUB-015            | testing | ✅ Done |
| TEST-006 | Integration test: Full proposal → vote → skip → refund flow                             | L    | 4   | 70K    | 1C    | VOTE-007, ECON-004 | testing | ✅ Done |

## Phase 1D–1G — E2E Tests

| ID       | Task                                                           | Size | PRs | Tokens | Phase | Deps    | Agent   | Status  |
| -------- | -------------------------------------------------------------- | ---- | --- | ------ | ----- | ------- | ------- | ------- |
| TEST-007 | Angular E2E: Hub discovery → join → propose → vote → skip flow | L    | 4   | 70K    | 1D    | WEB-014 | testing | ✅ Done |
| TEST-008 | Flutter E2E: Hub scan → propose → vote → skip flow             | L    | 4   | 70K    | 1D    | MOB-014 | testing | ✅ Done |

## Phase 1G — Final Quality Gates

| ID       | Task                                                                           | Size | PRs | Tokens | Phase | Deps      | Agent   | Status  |
| -------- | ------------------------------------------------------------------------------ | ---- | --- | ------ | ----- | --------- | ------- | ------- |
| TEST-009 | Coverage report generation (backend + web + mobile, merge into single report)  | M    | 2   | 40K    | 1G    | —         | testing | ✅ Done |
| TEST-010 | SonarQube CI integration (scanner in GitHub Actions, quality gate enforcement) | M    | 2   | 40K    | 1G    | INFRA-014 | testing | ✅ Done |

## Dependency Graph

```
TEST-001 (depends on WEB-001 — auth flow needed for test fixtures)
TEST-002 (depends on MOB-001 — auth flow needed for test fixtures)
TEST-003 (standalone — shared utilities)
TEST-004 (standalone — factory enhancement)
TEST-005 (depends on HUB-015 — attachment flow must exist)
TEST-006 (depends on VOTE-007 + ECON-004 — skip + refund must exist)
TEST-007 (depends on WEB-014 — web voting UI must exist)
TEST-008 (depends on MOB-014 — mobile voting UI must exist)
TEST-009 (standalone — report tooling)
TEST-010 (depends on INFRA-014 — SonarQube configured)
```
