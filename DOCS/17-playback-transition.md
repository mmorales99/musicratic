# Playback Transition

## Purpose

This file defines what happens when a suggested song becomes the next song to play.

It is the most concrete layer in this section because it focuses on the transition at playback time.

## Transition Rule

When a song finishes, the next song is played normally.

If the next song is a suggested song, it is removed from the queue when playback starts.

If the current top song is skipped or removed by the owner, the next song in the queue becomes the first one.

## Playback Path

1. The current song reaches its end.
2. The next song is selected.
3. If the next song came from the suggestion queue, it leaves the queue.
4. The song starts playing.
5. If a top song is skipped or removed, the queue shifts forward and the next song takes its place.

## MVP Scope

For the MVP, playback transition should be direct and deterministic.

## Scope Of This File

This file defines the final handoff from a candidate song to active playback.
