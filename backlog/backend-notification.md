# Backend Notification Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 10 |
| Done | 8 |
| Remaining | 2 |
| Est. premium requests | ~22 |
| Est. tokens | ~380 K |

## Phase 1B–1G — Notifications

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| NTFY-001 | Notification entity (user_id, type, title, body, data_json, read_at, created_at) | S | 1 | 15K | 1B | — | backend-module | ✅ Done |
| NTFY-002 | Notification EF configuration + NotificationRepository | S | 1 | 20K | 1B | NTFY-001 | database | ✅ Done |
| NTFY-003 | NotificationDbContext (schema "notification") + DI registration | M | 2 | 30K | 1B | NTFY-002 | database | ✅ Done |
| NTFY-004 | WebSocket notification delivery (push to connected user sessions) | M | 3 | 50K | 1B | NTFY-001 | backend-module | ✅ Done |
| NTFY-005 | APNs push notification service (direct HTTP/2 to Apple) | L | 4 | 70K | 1C | NTFY-001 | backend-module | ✅ Done |
| NTFY-006 | FCM push notification service (direct HTTP to Google) | L | 4 | 70K | 1C | NTFY-001 | backend-module | ✅ Done |
| NTFY-007 | Notification preferences entity + CRUD (per-user opt-in/out per type) | M | 2 | 35K | 1D | NTFY-001 | backend-module | ✅ Done |
| NTFY-008 | Notification API endpoints (list notifications, mark read, update preferences) | M | 2 | 35K | 1D | NTFY-004 | backend-module | ✅ Done |
| NTFY-009 | Dapr event handlers (vote_cast, track_skipped, review_created, report_ready → notify) | M | 3 | 50K | 1E | NTFY-004 | backend-module | 📋 Backlog |
| NTFY-010 | Email notification service (SMTP for weekly/monthly analytics reports) | M | 2 | 40K | 1E | NTFY-001 | backend-module | 📋 Backlog |

## Dependency Graph

```
NTFY-001 ──► NTFY-002 ──► NTFY-003
         ├─► NTFY-004 ──► NTFY-008
         │            └─► NTFY-009
         ├─► NTFY-005
         ├─► NTFY-006
         ├─► NTFY-007
         └─► NTFY-010
```
