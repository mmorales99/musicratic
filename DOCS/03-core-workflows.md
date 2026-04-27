# Core Workflows

## Purpose

This file describes the MVP flows that connect the actors.

It is more concrete than the actor definitions because it explains what happens over time.

## MVP Workflows

### 1. Shop Setup

1. The shop owner creates the music session.
2. The shop owner prepares the list of songs.
3. The shop owner selects the device that will play the music.
4. The shop owner starts the session.
5. The playing server becomes available for visitors.

### 2. Visitor Join

1. A visitor enters the shop.
2. The visitor joins the session by scanning a QR code, entering a code, or using the browser to find nearby sessions.
3. The visitor connects to the active playing server.
4. The visitor sees the current song and the available song suggestions.
5. The visitor stays joined only while the session is active and the join is still valid.

### 3. Song Suggestion And Voting

1. The visitor suggests a song or votes on the current song.
2. The shop owner and other visitors can vote on the suggested song.
3. The voting result determines whether the song moves forward.
4. If there is a tie, the shop owner resolves it.
5. The shop owner can use a vote to push a song forward or to pass it.

## Decision Rule

The shop owner has the maximum authority in the session.

That means the owner can:

- pass a suggested song
- break a tie
- decide the final direction of the session when votes are not enough

## Scope Of This File

This file turns the actor model into a concrete sequence of actions.

If more detail is needed, the next files should split these workflows into state changes, vote rules, and session rules.
