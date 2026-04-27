# Auth Flow

## Purpose

This file defines how authentication affects the visitor join path.

It is more concrete than the join lifecycle because it focuses on the app-level auth decision.

## Auth Rule

The user authenticates to the app, not to the session server.

When the visitor joins, the app sends a flag to the session server that indicates whether the user can suggest songs.

## Flow

1. The user opens the app.
2. The app checks whether the user is authenticated.
3. If the user is authenticated, the app marks the user as allowed to suggest songs.
4. If the user is not authenticated, the user starts as an unverified guest.
5. If the user accepts the join-time auth request, the user returns to guest.

## MVP Scope

The MVP only needs the auth state that controls song suggestion permission.

The session server only needs the resulting flag.

## Scope Of This File

This file defines the app-level authentication step that feeds the session join.
