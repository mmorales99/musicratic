# Owner Override Rules

## Purpose

This file defines the shop owner's override powers.

It is more concrete than the vote result rules because it focuses on authority behavior.

## Override Powers

The shop owner can:

- remove a song from the pending list
- skip a playing song at any time

## Queue Removal Rule

If the owner removes or skips the topmost song, that song leaves the queue.

The second song becomes the first, and the remaining songs shift forward in order.

## Authority Rule

The owner can act even when the normal vote flow has not finished.

That means the owner's decision takes priority over the queue result when needed.

## MVP Scope

For the MVP, the owner override should stay limited to removal and skip actions.

## Scope Of This File

This file defines how the owner can override the normal suggestion flow.
