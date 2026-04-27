# Auth And Suggestion Rules

## Purpose

This file defines who can suggest songs in each join state.

It is more concrete than the join lifecycle because it turns state names into permission rules.

## Suggestion Rules

### Unverified Guest

- cannot suggest songs

### Guest

- can suggest songs

### Pending To Join

- cannot suggest songs until the join response is resolved

### Joined

- can suggest songs

### Expired

- cannot suggest songs

## Authentication Rule

If a pending to join visitor accepts the server request to authenticate, the user returns to guest.

That means the user becomes eligible to suggest songs again.

## MVP Scope

For the MVP, keep the permission model simple.

The main distinction is whether the user is allowed to suggest songs or not.

## Scope Of This File

This file defines the permission side of the join lifecycle.
