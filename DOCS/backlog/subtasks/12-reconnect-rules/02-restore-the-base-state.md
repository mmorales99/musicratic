# Status

not-started

# Priority

medium

# Feature

Base state restoration

# Description

Restore the visitor to the base authenticated or unverified state when reconnecting.

# Acceptance Criteria

- Authenticated users reconnect as guests.
- Unauthenticated users reconnect as unverified guests.
- The restored state matches the app auth state.
