# Session Details

## Purpose

This file defines the data a visitor receives when joining a session.

It is the most concrete layer in this section because it specifies the session payload.

## Session Detail Payload

When the server accepts a join, it should provide:

- current playing song
- playlist
- available songs and sources
- vote state
- session code

## MVP Scope

The payload should stay small and focused on the music session.

Later versions can add more data if needed.

## Scope Of This File

This file defines the information sent to the visitor after a successful join.
