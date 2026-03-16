# Backend Economy Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 15 |
| Done | 4 |
| Remaining | 11 |
| Est. premium requests | ~34 |
| Est. tokens | ~600 K |

## Phase 1C — Voting & Skipping (Refunds)

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| ECON-001 | WalletTransaction entity (user_id, type [purchase/spend/refund], amount, reference_id) | S | 1 | 20K | 1C | — | backend-module | ✅ Done |
| ECON-002 | WalletTransaction EF configuration + WalletTransactionRepository | S | 1 | 20K | 1C | ECON-001 | database | ✅ Done |
| ECON-003 | EconomyDbContext (schema "economy") + DI registration | M | 2 | 30K | 1C | ECON-002 | database | ✅ Done |
| ECON-004 | Refund service (50% on skip, round down, credit wallet, record transaction) | M | 3 | 50K | 1C | ECON-001, VOTE-007 | backend-module | ✅ Done |

## Phase 1D — Economy

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| ECON-005 | Coin pricing engine (base = floor(duration/60), hotness multiplier, final = floor(base×mult)) | M | 3 | 50K | 1D | — | backend-module | 📋 Backlog |
| ECON-006 | Wallet transaction history query (paginated, filtered by type) | S | 1 | 20K | 1D | ECON-001 | backend-module | 📋 Backlog |
| ECON-007 | Coin purchase command via Stripe webhook (verify signature → credit wallet) | L | 4 | 70K | 1D | ECON-001 | backend-module | 📋 Backlog |
| ECON-008 | Stripe PaymentIntent creation endpoint (amount → payment intent) | M | 2 | 40K | 1D | — | backend-module | 📋 Backlog |
| ECON-009 | Apple IAP receipt verification (App Store Server API → validate → credit) | L | 4 | 70K | 1D | ECON-001 | backend-module | 📋 Backlog |
| ECON-010 | Google Play Billing verification (Google Play Developer API → validate → credit) | L | 4 | 70K | 1D | ECON-001 | backend-module | 📋 Backlog |
| ECON-011 | Subscription entity (hub_id, tier, started_at, expires_at, stripe_subscription_id) | S | 1 | 20K | 1D | — | backend-module | 📋 Backlog |
| ECON-012 | Subscription EF configuration + SubscriptionRepository | S | 1 | 20K | 1D | ECON-011 | database | 📋 Backlog |
| ECON-013 | Subscription tier enforcement service (check limits: hubs, lists, sub-owners per tier) | M | 2 | 40K | 1D | ECON-011 | backend-module | 📋 Backlog |
| ECON-014 | Free trial lifecycle (30d trial, prompts at day 20/28, deactivate at day 30, 90d retention) | M | 3 | 50K | 1D | ECON-013 | backend-module | 📋 Backlog |
| ECON-015 | Economy API endpoints (get wallet, get transactions, create payment, manage subscription) | M | 2 | 40K | 1D | ECON-005 | backend-module | 📋 Backlog |

## Dependency Graph

```
ECON-001 ──► ECON-002 ──► ECON-003
         ├─► ECON-004 (also needs VOTE-007)
         ├─► ECON-006
         ├─► ECON-007
         ├─► ECON-009
         └─► ECON-010
ECON-005 ──► ECON-015
ECON-008 (standalone Stripe setup)
ECON-011 ──► ECON-012
         └─► ECON-013 ──► ECON-014
```
