# Main Actors

## Purpose

This file identifies the main actors that interact with the product.

An actor is any person, device, system, or external service that plays a role in the workflow.

## Main Actor Set

For the gramola MVP, the main actors are:

- shop owner
- visitor
- playing device
- playing server
- song catalog or suggested song source

## Actor Roles

### Shop Owner

- prepares the song list for the shop
- selects the device that will play the music
- starts the session
- keeps the final authority over song decisions

### Visitor

- joins the shop session
- listens to the current music
- suggests songs
- votes on the current song and on suggested songs

### Playing Device

- outputs the music in the shop
- receives playback commands from the session owner

### Playing Server

- listens for visitors to join
- receives suggestions and votes
- coordinates the current session state

### Song Catalog Or Suggested Song Source

- provides the songs that can be selected or suggested in the session
- gives the session a bounded set of options for the MVP

## What Each Actor Owns

For every actor, define:

- what they do
- what they need from the system
- what the system expects from them
- which parts of the product they can access

## Scope Of This File

This file stays one level lower than the product description.

It does not go into internal structure yet. It only names the actors and explains their role in the system.
