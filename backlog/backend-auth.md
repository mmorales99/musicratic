# Backend Auth Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 16 |
| Done | 14 |
| Remaining | 2 |
| Est. premium requests | ~22 |
| Est. tokens | ~400 K |

## Phase 0 — Foundation (DONE)

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| AUTH-001 | User entity + domain events | S | 1 | 15K | 0 | — | backend-module | ✅ Done |
| AUTH-002 | CreateUser + UpdateProfile + CreditWallet commands | M | 2 | 35K | 0 | — | backend-module | ✅ Done |
| AUTH-003 | GetUserBySub + GetUserById queries | S | 1 | 15K | 0 | — | backend-module | ✅ Done |
| AUTH-004 | AuthDbContext + UserConfiguration + UserRepository | M | 2 | 35K | 0 | — | database | ✅ Done |
| AUTH-005 | UserEndpoints (GET user, GET me, PUT me) | S | 1 | 15K | 0 | — | backend-module | ✅ Done |
| AUTH-006 | User domain + handler unit tests (37 tests) | M | 3 | 50K | 0 | — | testing | ✅ Done |

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| AUTH-010 | Authentik OIDC discovery + options configuration | M | 2 | 35K | 1A | — | backend-module | ✅ Done |
| AUTH-011 | Login endpoint (redirect to Authentik authorize URL) | S | 1 | 20K | 1A | AUTH-010 | backend-module | ✅ Done |
| AUTH-012 | OIDC callback handler (exchange code → tokens → create/update user) | L | 4 | 70K | 1A | AUTH-010 | backend-module | ✅ Done |
| AUTH-013 | JWE access token validation middleware | M | 3 | 50K | 1A | AUTH-010 | backend-module | ✅ Done |
| AUTH-014 | Refresh token endpoint (exchange opaque refresh → new JWE) | M | 2 | 35K | 1A | AUTH-013 | backend-module | ✅ Done |
| AUTH-015 | Logout endpoint (revoke Authentik session + clear tokens) | S | 1 | 20K | 1A | AUTH-013 | backend-module | ✅ Done |
| AUTH-016 | Avatar upload endpoint (stream to Azurite blob, update user) | M | 2 | 40K | 1A | — | backend-module | ✅ Done |

## Phase 1F — Roles & Delegation

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| AUTH-020 | Role resolution service (query HubMember per request → resolve role) | M | 3 | 50K | 1F | — | backend-module | ✅ Done |
| AUTH-021 | Authorization middleware (role ≥ required → allow, else 403 ProblemDetails) | M | 2 | 40K | 1F | AUTH-020 | backend-module | 📋 Backlog |
| AUTH-022 | RequireRole attribute/endpoint filter for minimal APIs | S | 1 | 20K | 1F | AUTH-021 | backend-module | 📋 Backlog |

## Dependency Graph

```
AUTH-010 ──► AUTH-011
         ├─► AUTH-012
         ├─► AUTH-013 ──► AUTH-014
         │            └─► AUTH-015
AUTH-020 ──► AUTH-021 ──► AUTH-022
```
