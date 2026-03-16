# 06 — Monetization Model

## Revenue Streams Overview

Musicratic has four revenue streams, designed to scale from free-tier adoption to paid power usage.

| Stream | Payer | Model |
|---|---|---|
| **Virtual Coins** | Visitors (B2C) | Microtransaction — pay per song proposal |
| **Hub Subscriptions** | Venue owners (B2B) | Annual / Monthly / Event plans |
| **Mood Events** | Social users (B2C) | Time + user-count based pricing |
| **Advertisements** | Free-tier users (B2C/B2B) | Ad impressions on unpaid hubs |

---

## 1. Virtual Coins

Virtual coins are the in-app currency visitors use to propose tracks immediately (bypassing collective vote).

### Coin Pricing for Track Proposals

A track's coin cost is determined by two factors:

#### Factor A — Duration (Base Cost)

$$\text{base\_cost} = \left\lfloor \frac{\text{duration\_seconds}}{60} \right\rfloor$$

| Track Duration | Base Cost |
|---|---|
| 0:00 – 1:59 | 1 coin |
| 2:00 – 2:59 | 2 coins |
| 3:00 – 3:59 | 3 coins |
| 4:00 – 4:59 | 4 coins |
| 5:00 – 5:59 | 5 coins |
| ... | ... |

> Always rounds down. A 2:30 track costs 2 coins (not 3).

#### Factor B — Hotness Multiplier

Hotness is a global metric measuring a track's concurrent reproduction rate across all active hubs:

$$\text{hotness} = \frac{\text{concurrent\_plays\_across\_all\_hubs}}{\text{total\_active\_hubs}}$$

| Hotness Range | Multiplier | Label |
|---|---|---|
| 0.00 – 0.01 | x1.0 | Normal |
| 0.01 – 0.05 | x1.25 | Warm |
| 0.05 – 0.15 | x1.5 | Hot |
| 0.15 – 0.30 | x2.0 | Fire |
| > 0.30 | x2.5 | Viral |

#### Final Cost

$$\text{coin\_cost} = \left\lfloor \text{base\_cost} \times \text{hotness\_multiplier} \right\rfloor$$

> Always rounds down after multiplication.

**Examples:**

| Track | Duration | Hotness | Base | Multiplier | Final Cost |
|---|---|---|---|---|---|
| Indie ballad | 3:45 | 0.002 (Normal) | 3 | x1.0 | **3 coins** |
| Pop hit | 3:20 | 0.08 (Hot) | 3 | x1.5 | **4 coins** |
| Viral TikTok song | 2:15 | 0.35 (Viral) | 2 | x2.5 | **5 coins** |

### Coin Purchase Packages

| Package | Coins | Price (EUR) | Bonus |
|---|---|---|---|
| Starter | 10 | 0.99 | — |
| Regular | 30 | 1.99 | +5 free coins |
| Party Pack | 75 | 3.99 | +15 free coins |
| Mega Pack | 200 | 7.99 | +50 free coins |

Coins do not expire. No real-money-back refund for unused coins (standard virtual currency policy for app stores).

### Refund Rules (In-Coin)

As defined in the [Voting & Playback Rules](05-voting-and-playback.md):

- Track skipped by vote threshold (≥65%) → **50% coin refund** (rounded down).
- Track skipped by owner → **50% coin refund** (rounded down).
- Track played to completion → **no refund**.

---

## 2. Hub Subscriptions (Venue Owners)

Hub subscriptions unlock the full venue management experience.

### Tier Comparison

| Feature | Free Trial | Monthly | Annual |
|---|---|---|---|
| **Price** | €0 (30 days) | €14.99/mo | €119.99/yr (save 33%) |
| **Duration** | 30 days | Rolling | Rolling |
| Hub creation | 1 hub | Up to 3 hubs | Up to 10 hubs |
| Lists per hub | 1 | 5 | Unlimited |
| Sub-owners / managers | 0 | 2 | 10 |
| Music sources | Spotify only | Spotify + YouTube | All + local storage |
| Local track storage | No | No | Yes (up to 10 GB) |
| Analytics reports | Basic (this week) | Full (weekly + monthly) | Full + CSV export |
| Ads shown to visitors | Yes | No | No |
| Custom QR branding | No | No | Yes |
| API access | No | No | Yes |
| Priority support | No | No | Yes |

### Event License

For one-off events (festival, wedding, corporate):

| Duration | Price (EUR) |
|---|---|
| 6 hours | €9.99 |
| 12 hours | €14.99 |
| 24 hours | €19.99 |
| 48 hours | €29.99 |

Event licenses include all Annual features for the event duration.

### Free Trial → Conversion

- 30-day free trial, no credit card required.
- At day 20, prompt the owner with conversion offer and usage stats.
- At day 28, warn that trial expires in 2 days.
- At day 30, hub becomes inactive. Data retained for 90 days. Reactivation restores everything.

---

## 3. Mood Events (Portable Hubs)

Portable hubs (Phase 2) are priced per-event based on **duration** and **user count**.

### Base Pricing

| Users | 24h Price (EUR) |
|---|---|
| Up to 5 | 5.00 |
| Up to 7 | 6.67 (~+33%) |
| Up to 9 | 8.33 (~+33%) |
| Up to 11 | 10.00 (~+33%) |
| ... | +33% per 2 extra users |

### Duration Scaling

Each duration doubling increases cost by **35%** (not 100%):

| Duration | Multiplier | Price for 5 users |
|---|---|---|
| 24h | x1.00 | €5.00 |
| 48h | x1.35 | €6.75 |
| 72h | x1.82 | €9.10 |
| 96h | x2.46 | €12.29 |

**General formula:**

$$\text{price} = \text{base}(u) \times 1.35^{\left(\frac{h}{24} - 1\right)}$$

Where:
- $\text{base}(u)$ = base price for $u$ user slots
- $h$ = total hours

**During mood events**: All attached users play songs for **0 coins** (free proposals). This is baked into the event price.

### Free Mood Tier

- 1 mood event per month.
- Max 3 users, max 2 hours.
- Shows 1 ad per hour to all participants.

---

## 4. Advertisements

Ads appear in the free tier and for unpaid hub visitors.

### Ad Placement Rules

| Context | Frequency | Format |
|---|---|---|
| **Free hub, list tracks** | 1 audio ad per hour | 15-30 second audio spot between tracks |
| **Free hub, visitor proposals** | 1 audio ad after each non-list-owner proposed track | 15-30 second audio spot after track ends |
| **Free mood event** | 1 audio ad per hour | 15-30 second audio spot between tracks |
| **In-app** | Banner on queue screen | Visual only, no audio interruption |

### Ad-Free Conditions

No ads when:
- Hub has a paid subscription (Monthly or Annual).
- Mood event is paid.
- User has purchased any coin package in the last 30 days (reward for payers).

---

## Revenue Projections (Year 1 Estimate)

| Stream | Assumption | Monthly Revenue |
|---|---|---|
| Virtual coins | 5,000 MAU × 10% buy × €2 avg purchase | €1,000 |
| Hub subscriptions | 50 venues × €15/mo avg | €750 |
| Mood events | 200 events/mo × €6 avg | €1,200 |
| Ads | 40,000 ad impressions × €2 CPM | €80 |
| **Total** | | **~€3,030/mo** |

> These are conservative estimates for a bootstrapped launch targeting a single metro area.

---

## Payment Infrastructure

| Component | Provider | Notes |
|---|---|---|
| In-app purchases (coins) | Apple IAP / Google Play Billing (direct) | Required by app store policies. No middleware (RevenueCat etc.) — integrate directly to avoid subscription fees. |
| Hub subscriptions (web) | Stripe | Pay-per-transaction only (no monthly platform fee). |
| Hub subscriptions (mobile) | Apple IAP / Google Play Billing (direct) | Same direct integration as coins. |
| Mood events | Same as subscriptions | Treated as one-time IAP or Stripe checkout. |
| Ad network | Self-served or deferred | Ads are a Phase 2 revenue stream. If implemented, evaluate open-source ad-serving before committing to AdMob. |

**Cost philosophy**: The only third-party payment dependencies are Stripe (unavoidable for web payments) and Apple/Google IAP (mandated by stores). No intermediary services like RevenueCat — the integration effort is worth the €0/month cost.

**Important**: Apple and Google take a 15-30% commission on in-app purchases. Coin pricing accounts for this.
