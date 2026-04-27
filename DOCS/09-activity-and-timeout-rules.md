# Activity And Timeout Rules

## Purpose

This file defines what counts as activity for the timeout rule.

It is more concrete than the session lifecycle because it focuses on timer refresh rules.

## Activity Rules

The timer is refreshed when the visitor does any of these actions:

- suggests a song
- votes on a song
- opens the playlist

## Timeout Rule

If the visitor does not do any of those actions for one hour, the user is considered inactive.

When that happens, the session can disconnect the user.

## MVP Scope

The MVP uses activity-based timing only.

GPS is out of scope.

## Scope Of This File

This file defines the activity that keeps a visitor session alive.
