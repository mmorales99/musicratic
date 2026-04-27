# Active Session States

## Purpose

This file defines the session states for the shop session itself.

It is more concrete than the join lifecycle because it focuses on the session runtime.

## Session States

### Dead

- the session is not running
- the shop may still be configured
- this is the state before the play button starts the session
- this is also the state after the session is closed

### Live

- the session is running
- visitors can join and interact with the music session

## State Flow

1. The server is configured for a session.
2. The session is considered dead while configuration is happening.
3. The shop owner presses play.
4. The session transitions to live.
5. The shop owner presses close.
6. The session transitions back to dead.

## MVP Scope

For the MVP, keep the session state model simple.

The only required runtime states are dead and live.

## Scope Of This File

This file describes how the session itself moves between runtime states.
