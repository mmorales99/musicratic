# Expiration And Reconnect Rules

## Purpose

This file defines when a visitor loses access and how they can join again.

It is the most concrete layer in this section because it focuses on timing and reconnect behavior.

## Expiration Rule

A visitor is considered inactive if they do not do any of these actions for one hour:

- suggest a song
- vote on a song
- request the playlist

When that happens, the user is disconnected from the session.

## Reconnect Rule

After disconnecting for inactivity, the visitor can rejoin after one minute.

## MVP Scope

GPS is not part of the MVP.

The MVP uses activity-based expiration only.

## Scope Of This File

This file captures the timeout and reconnect behavior for the gramola MVP.
