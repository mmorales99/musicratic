# Tie Break Rules

## Purpose

This file defines how ties are resolved.

It is more concrete than the vote result rules because it focuses on the configured tie-break behavior.

## Tie-Break Options

The shop owner configures one of these options:

- downvote priority
- upvote priority
- shop owner required

## Tie-Break Rule

If the vote result is tied, the configured option decides the outcome.

If the configuration is shop owner required, the owner must resolve the tie.

## MVP Scope

For the MVP, the tie-break configuration should be explicit and predictable.

## Scope Of This File

This file defines how tied votes are resolved before the next playback decision.
