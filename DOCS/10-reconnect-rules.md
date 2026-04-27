# Reconnect Rules

## Purpose

This file defines what happens when a disconnected visitor returns.

It is the most concrete layer in this section because it focuses on how the user comes back after timeout.

## Reconnect Rule

After a disconnect for inactivity, the visitor can rejoin after one minute.

## Base State Rule

When the visitor reconnects, the user returns to the base state:

- if the user is not authenticated, the user becomes unverified
- if the user is authenticated, the user becomes a guest

The rejoin process is the same as the first join.

## MVP Scope

The MVP does not use GPS for reconnect decisions.

It uses the same session entry flow every time.

## Scope Of This File

This file defines the reconnect path after inactivity disconnects.
