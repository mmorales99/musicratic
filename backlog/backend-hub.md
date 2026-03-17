# Backend Hub Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 22 |
| Done | 19 |
| Remaining | 3 |
| Est. premium requests | ~35 |
| Est. tokens | ~600 K |

## Phase 0 — Foundation (DONE)

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| HUB-001 | Hub + HubMember + HubAttachment + List + ListTrack entities | L | 3 | 60K | 0 | — | backend-module | ✅ Done |
| HUB-002 | Hub commands (Create, Activate, Deactivate, Attach/Detach, CreateList, AddTrack) | L | 3 | 60K | 0 | — | backend-module | ✅ Done |
| HUB-003 | Hub queries (GetHub, GetActiveHubs, GetHubMembers) | M | 2 | 30K | 0 | — | backend-module | ✅ Done |
| HUB-004 | HubDbContext + 5 entity configurations + 4 repositories | L | 4 | 70K | 0 | — | database | ✅ Done |
| HUB-005 | HubEndpoints + AttachmentEndpoints + ListEndpoints | M | 2 | 35K | 0 | — | backend-module | ✅ Done |
| HUB-006 | Hub domain + handler unit tests (37 tests) | M | 3 | 50K | 0 | — | testing | ✅ Done |
| HUB-007 | HubSettings value object (JSON-stored, configurable fields) | S | 1 | 20K | 0 | — | backend-module | ✅ Done |

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| HUB-010 | Hub code generation service (unique short alphanumeric, e.g. CAFELUNA23) | S | 1 | 20K | 1A | — | backend-module | ✅ Done |
| HUB-011 | QR code generation service (signed URL + QR image → Azurite blob) | M | 3 | 50K | 1A | HUB-010 | backend-module | ✅ Done |
| HUB-012 | Deep link generation (https://musicratic.app/join/{code}?sig={sig}) | S | 1 | 20K | 1A | HUB-010 | backend-module | ✅ Done |
| HUB-013 | Hub update + soft-delete commands | M | 2 | 35K | 1A | — | backend-module | ✅ Done |
| HUB-014 | Hub settings CRUD (update individual settings via command) | M | 2 | 35K | 1A | — | backend-module | ✅ Done |
| HUB-015 | Attachment flow (validate hub active → detach previous → create with 1h expiry) | L | 4 | 70K | 1A | AUTH-013 | backend-module | ✅ Done |
| HUB-016 | Attachment expiry background job (check expired → auto-detach) | M | 2 | 40K | 1A | HUB-015 | backend-module | ✅ Done |
| HUB-017 | List update + delete + reorder commands | M | 2 | 35K | 1A | — | backend-module | ✅ Done |
| HUB-018 | ListTrack management (add, remove, reorder, bulk-add) | M | 2 | 40K | 1A | — | backend-module | ✅ Done |
| HUB-019 | Hub search/filter query (by name, type, visibility, tenant) | M | 2 | 35K | 1A | — | backend-module | ✅ Done |
| HUB-020 | Play mode logic (ordered sequence vs weighted shuffle per list) | M | 2 | 40K | 1A | — | backend-module | ✅ Done |
| HUB-021 | Hub pause / resume state transition commands | S | 1 | 20K | 1A | — | backend-module | ✅ Done |

## Phase 1F — Roles & Delegation

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| HUB-030 | Hub member list + detail queries (with role info) | S | 1 | 20K | 1F | — | backend-module | ✅ Done |
| HUB-031 | Promote / demote member commands (with tier limits enforcement) | M | 2 | 40K | 1F | AUTH-020 | backend-module | 📋 Backlog |
| HUB-032 | Permission scoping (sub-list-owner sees only assigned lists) | M | 2 | 35K | 1F | AUTH-021 | backend-module | 📋 Backlog |

## Dependency Graph

```
HUB-010 ──► HUB-011
         └─► HUB-012
HUB-015 ──► HUB-016          (HUB-015 also depends on AUTH-013)
HUB-030 ──► HUB-031 ──► HUB-032   (HUB-031 also depends on AUTH-020)
```
