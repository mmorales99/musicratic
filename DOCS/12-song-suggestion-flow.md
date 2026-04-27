# Song Suggestion Flow

## Purpose

This file defines how a visitor suggests a song and how that suggestion enters the session.

It is more concrete than the core workflow file because it focuses on the song queue and playback timing.

## Suggestion Flow

1. The visitor opens the available song list provided by the server.
2. The visitor selects a song to suggest.
3. The suggestion enters the FIFO list as pending.
4. The session keeps the suggestion available for later voting.
5. When there are 30 seconds remaining, the top pending song becomes the next candidate.

## Voting Timing

The vote flow follows playback time:

- while the current song is playing, voting starts during the last 30 seconds
- when the suggested song starts, voting ends during the first 15 seconds

## MVP Scope

For the MVP, keep the suggestion queue simple and deterministic.

The server provides the song sources and the available songs.

## Scope Of This File

This file defines the path from song selection to pending suggestion.
