# Vote Result Rules

## Purpose

This file defines what the vote result does to a suggested song.

It is more concrete than the general vote rules because it applies the result to playback.

## Vote Outcomes

### More Positive

- the song keeps playing

### More Negative

- the song is skipped

## Decision Rule

The vote result is applied to the current candidate or the current song depending on the session state.

The result should be simple:

- positive outcome keeps the song moving forward
- negative outcome removes the song from the current path

## Vote Counting Rule

Votes are either up or down.

The option with the most votes wins.

## MVP Scope

For the MVP, keep vote outcomes binary and easy to understand.

## Scope Of This File

This file defines how vote results affect the song path.
