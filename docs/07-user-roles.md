# 07 — User Roles & Permissions

## Role Hierarchy

Musicratic uses a **5-tier, accumulative role system**. Each higher role inherits all permissions from the roles below it. A user's permissions depend on their role within a specific hub. A user can hold different roles in different hubs.

```
                    ┌─────────────────────┐
                    │   Hub Manager (5)   │  ← Hub operations & config
                    └─────────┬───────────┘
                              │ inherits all below
                    ┌─────────────────────┐
                    │   List Owner (4)    │  ← Content curation & playback control
                    └─────────┬───────────┘
                              │ inherits all below
                    ┌─────────────────────┐
                    │      User (3)       │  ← Paying participant
                    └─────────┬───────────┘
                              │ inherits all below
                    ┌─────────────────────┐
                    │    Visitor (2)      │  ← Joins hubs, votes
                    └─────────┬───────────┘
                              │ inherits all below
                    ┌─────────────────────┐
                    │   Anonymous (1)     │  ← Browse only (no auth)
                    └─────────────────────┘
```

**Accumulative** means: a Hub Manager can do everything a List Owner can do, plus Hub Manager-specific actions. A User can do everything a Visitor can do, plus User-specific actions. And so on.

## Role Definitions

### Anonymous (Level 1) — No Authentication Required

Unauthenticated users who browse the platform without logging in.

- **How assigned**: Default for any unauthenticated request.
- **Scope**: Read-only browsing of public hub data.
- **Allowed actions**:
  - List active hubs
  - View featured hubs
  - View hub public profiles (name, genre, ratings)

### Visitor (Level 2) — Authenticated, Attached to Hub

A consumer who joins a hub to listen, interact, and vote.

- **How assigned**: Default role upon attaching to a hub (requires authentication).
- **Scope**: Participates in hub activity but cannot spend money or manage content.
- **New actions (in addition to Anonymous)**:
  - Join / leave hubs
  - View live queue
  - Upvote / downvote tracks
  - Browse queue history
  - View real-time vote tallies

### User (Level 3) — Paying Participant

A registered user who can spend coins and request content.

- **How assigned**: Any authenticated user who has completed profile setup.
- **Scope**: Financial interactions and content proposals.
- **New actions (in addition to Visitor)**:
  - Propose tracks (costs coins)
  - Buy coins (via Stripe / IAP)
  - Start a mood event (Phase 2)
  - Propose stop (request to end current track)
  - View own wallet and transaction history
  - Leave reviews on hubs
  - Create portable hubs (Phase 2)

### List Owner (Level 4) — Content Curator & Playback Controller

A user responsible for managing playlists and controlling playback within a hub.

- **How assigned**: Promoted by the Hub Manager or hub creator.
- **Limit**: Based on subscription tier (0 for free trial, 2 for monthly, 10 for annual).
- **Scope**: Full control over assigned lists and playback.
- **New actions (in addition to User)**:
  - Set default track list
  - Add / remove songs from assigned lists
  - Rewind current track
  - Skip current track (manual)
  - Stop playback
  - Change active track list
  - Set play mode (order / shuffle) for assigned lists
  - Owner downvote = instant skip (for tracks in assigned lists)
  - View analytics for assigned lists
  - Receive weekly/monthly reports for assigned lists

### Hub Manager (Level 5) — Hub Operations & Configuration

The ultimate authority over a hub's operational settings and user management.

- **How assigned**: Hub creator gets this role automatically. Can promote others.
- **One creator per hub**: The original creator cannot be demoted (only hub deletion removes them).
- **Scope**: Full hub administration.
- **New actions (in addition to List Owner)**:
  - Edit hub settings (name, description, genre tags, visibility)
  - Activate / deactivate hub
  - Delete hub (creator only)
  - Manage subscription / billing
  - Generate / rotate QR codes
  - Promote users to List Owner or Hub Manager
  - Revoke any role
  - Ban / unban users
  - Respond to reviews
  - Toggle hub visibility (public/private)
  - Mark live music happening
  - View full hub analytics
  - Export analytics (annual tier)
  - Create / delete lists

## Permission Matrix

| Permission | Anonymous | Visitor | User | List Owner | Hub Manager |
|---|---|---|---|---|---|
| **Browsing** | | | | | |
| List active hubs | Yes | Yes | Yes | Yes | Yes |
| View featured hubs | Yes | Yes | Yes | Yes | Yes |
| View hub public profile | Yes | Yes | Yes | Yes | Yes |
| **Hub Participation** | | | | | |
| Join hub | — | Yes | Yes | Yes | Yes |
| Leave hub | — | Yes | Yes | Yes | Yes |
| View live queue | — | Yes | Yes | Yes | Yes |
| Upvote / downvote | — | Yes | Yes | Yes | Yes |
| **Paid Actions** | | | | | |
| Propose track (coins) | — | — | Yes | Yes | Yes |
| Buy coins | — | — | Yes | Yes | Yes |
| Start mood event | — | — | Yes (Phase 2) | Yes | Yes |
| Propose stop | — | — | Yes | Yes | Yes |
| Leave review | — | — | Yes | Yes | Yes |
| **Content Control** | | | | | |
| Set default track list | — | — | — | Assigned | Yes |
| Add / remove songs | — | — | — | Assigned | Yes |
| Rewind | — | — | — | Yes | Yes |
| Skip (manual) | — | — | — | Yes | Yes |
| Stop playback | — | — | — | Yes | Yes |
| Change track list | — | — | — | Assigned | Yes |
| Owner downvote = instant skip | — | — | — | Assigned list | All tracks |
| **Hub Administration** | | | | | |
| Edit hub settings | — | — | — | — | Yes |
| Activate / deactivate hub | — | — | — | — | Yes |
| Delete hub | — | — | — | — | Creator only |
| Manage billing | — | — | — | — | Yes |
| Generate / rotate QR | — | — | — | — | Yes |
| Promote / revoke roles | — | — | — | — | Yes |
| Ban / unban users | — | — | — | — | Yes |
| **Analytics** | | | | | |
| View hub analytics | — | — | — | Assigned lists | Full |
| Weekly / monthly reports | — | — | — | Assigned lists | Full |
| Export analytics (CSV) | — | — | — | — | Annual tier |

## Access Control Enforcement

### Endpoint Protection

**Every endpoint must resolve access for the current user.** There are three access levels:

| Access Level | Token Required | How Resolved |
|---|---|---|
| **Anonymous** | No | No `Authorization` header needed. Limited to read-only hub listing. |
| **Authenticated** | Yes (JWE) | Bearer token decrypted + validated. User identity established. |
| **Hub-Scoped** | Yes (JWE) + hub context | Token + `hub_id` from route/header → role lookup in `HubMember` table. |

### Client Origin Restriction

**Only web app and mobile app clients can interact with action endpoints.** Non-browser / non-app HTTP requests are rejected:
- Web: validated via `Origin` / `Referer` headers + CORS policy.
- Mobile: validated via app attestation (Android SafetyNet / iOS DeviceCheck).
- JSON-RPC and GraphQL endpoints are not accessible from arbitrary HTTP clients.

## Authentication & Authorization Flow

### Authentication — Authentik (Identity Provider)

All authentication is delegated to **Authentik**, a self-hosted identity provider.

**Supported login methods:**

| Method | Notes |
|---|---|
| **Google** (OAuth2) | Social login — "Sign in with Google" |
| **Apple** (OAuth2) | Social login — "Sign in with Apple" (required for iOS) |
| **Spotify** (OAuth2) | Social login + automatic Spotify account linking for playback |
| **Email + Password** | Classic registration and login (Authentik-native) |

**Post-login security (MFA):**

| Method | Notes |
|---|---|
| **Passkeys** (WebAuthn/FIDO2) | Biometric / hardware key. Preferred. |
| **TOTP** | Time-based one-time passwords (Google Authenticator, Authy). |
| **Recovery codes** | One-time fallback for account recovery. |

- MFA is **optional** for Anonymous and Visitor roles. Encouraged via UI.
- MFA is **required** for List Owners and Hub Managers (they control billing and playlists).

**Token strategy — JWE + Refresh Tokens:**

| Token | Format | Lifetime | Purpose |
|---|---|---|---|
| **Access token** | JWE (encrypted JWT) | 15 min | Bearer token on every request. Encrypted — only backend can read claims. |
| **Refresh token** | Opaque (stored server-side) | 30 days | Renew access token without re-login. |

1. User authenticates via Authentik (OIDC redirect) → BFF exchanges code for JWE access_token + refresh_token.
2. Client stores tokens securely (HttpOnly cookie for web / secure storage for mobile).
3. Role claims are **not** embedded in the token — they are resolved per-request based on the user's hub context.
4. BFF decrypts JWE → validates claims → forwards to backend via gRPC.
5. On token expiry: client uses refresh token to get a new access token.

**Spotify account linking:**
- When a user logs in via Spotify, Authentik preserves the OAuth tokens. The backend uses them for Spotify API playback.
- Users who log in via Google/Apple/email can link Spotify separately in settings.

### Authorization

1. Every API request includes the `access_token` as `Bearer` token (except Anonymous endpoints).
2. The BFF decrypts the JWE and validates claims.
3. For hub-scoped actions, the request includes the `hub_id` (tenant context).
4. The authorization middleware:
   a. Resolves the tenant context from the hub (session/activity isolation only).
   b. Looks up the user's `HubMember` record for that hub.
   c. Determines the user's role (Anonymous → Visitor → User → List Owner → Hub Manager).
   d. Checks if the role level meets the minimum required for the action.
   e. Allows or denies the action.

```
Request: JSON-RPC { "method": "playback.propose_track", "params": { "hub_id": "uuid", "track_id": "...", "source": "coins" } }
         Authorization: Bearer {JWE access_token}

   |
   v
BFF: decrypt JWE -> validate claims -> extract user_id (authentik_sub)
   |
   v
Tenant Middleware: resolve tenant session from hub_id
   |
   v
Auth Middleware: lookup HubMember(user_id, hub_id) -> role = "user" (level 3)
   |
   v
Permission Check: "propose_track" requires level 3 (User) -> GRANTED
   |
   v
Hub Module: is user attached to this hub? -> YES
   |
   v
Economy Module (gRPC): does user have enough coins? -> YES
   |
   v
Playback Module (gRPC): add to queue -> success
```

## Role Assignment API

| Endpoint | Description | Required Role |
|---|---|---|
| `POST /hubs/{id}/members/{userId}/promote` | Assign list_owner or hub_manager role | Hub Manager |
| `DELETE /hubs/{id}/members/{userId}/role` | Revoke role (user becomes visitor) | Hub Manager |
| `POST /hubs/{id}/members/{userId}/ban` | Ban user from hub | Hub Manager |
| `DELETE /hubs/{id}/members/{userId}/ban` | Unban user | Super Owner, Sub Manager |
| `GET /hubs/{id}/members` | List all members with roles | Super Owner, Sub Manager |

## Mood-Specific Roles (Phase 2)

Portable hubs introduce additional role behaviors:

| Mood | Special Roles | Notes |
|---|---|---|
| **Home Party** | **Composer** (replaces owner) | Sets default list, votes and counts as normal user. No owner skip privileges. |
| **Driving** | **Driver** | Selects list, tracks are not skippable by others. Can only downvote. Special skip-limit rules (see [Mood System](08-mood-system.md)). |
