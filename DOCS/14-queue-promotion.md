# Queue Promotion

## Purpose

This file defines how a pending song becomes the next candidate.

It is more concrete than the suggestion flow because it focuses on queue movement.

## Promotion Rule

When there are 30 seconds remaining in the current song, the top pending song becomes the next candidate.

## Queue Rule

The suggestion queue works as FIFO.

That means the first pending suggestion is the first one considered for promotion.

## MVP Scope

For the MVP, queue promotion should be predictable and easy to follow.

The server only needs to promote the next candidate when the timing threshold is reached.

## Scope Of This File

This file defines how pending suggestions move into candidate status.
