# Actor Structure

## Purpose

This file breaks each actor into a more concrete structure.

Use it to explain how an actor is organized internally and how it connects to the rest of the system.

## Structure Pattern

For each actor, document:

- identity
- responsibilities
- inputs
- outputs
- permissions
- dependencies
- related workflows

## Example Structure

### Shop Owner

- identity: person who controls the session in the shop
- responsibilities: configures the playlist, selects the device, starts playback, and resolves ties
- inputs: song list, device selection, start action, up/down votes
- outputs: active session, playback commands, final song choice when needed
- permissions: can approve, reject, or break ties on song decisions
- dependencies: session UI, playback device, vote aggregation, server state

### Visitor

- identity: person in the shop who joins the active music session
- responsibilities: listens, suggests songs, and votes on songs
- inputs: join action, song suggestion, like or dislike vote
- outputs: suggestions, votes, participation in ranking decisions
- permissions: can suggest songs and vote within the active shop session
- dependencies: visitor client, active server session, song catalog or suggestion flow

### Playing Server

- identity: runtime service that coordinates the live music session
- responsibilities: accepts visitors, stores suggestions, records votes, and exposes session state
- inputs: start command, visitor joins, song suggestions, votes
- outputs: current song state, candidate song list, vote results, session updates
- permissions: can accept or reject actions based on session state
- dependencies: owner session setup, active device, network connectivity, persistence if needed

### Playing Device

- identity: the device selected by the shop owner to play music
- responsibilities: plays the current song and follows session playback commands
- inputs: playback commands, selected songs, session state updates
- outputs: audible playback in the shop, playback status
- permissions: plays only the music assigned by the session
- dependencies: audio output, network connection, playback integration

## Scope Of This File

This file is intentionally more concrete than the previous ones.

If a later document is needed, it should go deeper into one actor, one workflow, or one subsystem at a time.
