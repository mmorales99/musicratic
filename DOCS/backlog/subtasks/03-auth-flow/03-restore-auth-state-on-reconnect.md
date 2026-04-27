# Status

not-started

# Priority

high

# Feature

Auth state restoration

# Description

Restore the user's base auth state when they reconnect.

# Acceptance Criteria

- An authenticated user reconnects as a guest.
- An unauthenticated user reconnects as an unverified guest.
- The reconnect path uses the same auth result as the first join.
