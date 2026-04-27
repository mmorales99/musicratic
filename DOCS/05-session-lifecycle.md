# Session Lifecycle

## Purpose

This file defines the visitor join states for the gramola MVP.

It is more concrete than the workflow file because it focuses on how a visitor moves from first contact to an active join.

## Join States

### Unverified Guest

- the user is not authenticated
- the user cannot suggest songs

### Guest

- the user is authenticated
- the user can suggest songs

### Pending To Join

- the user enters the session by QR code, session code, or browser discovery
- the server receives the join request and sends session details such as the current song and the available playlist or sources
- the server asks the user to authenticate for suggesting songs
- the user stays in this state while the response is still pending

### Joined

- the user has finished the join response step
- the session treats the user as fully joined

### Expired

- the user has shown no activity for a period of time
- the join is no longer valid

## Join Flow

1. A user arrives as an unverified guest if they are not authenticated.
2. An authenticated user is treated as a guest.
3. The user joins by QR code, code, or browser discovery.
4. The server responds with session details and sends the suggestion permission flag.
5. While the response is pending, the user is in pending to join.
6. If the user accepts the authentication request, the user returns to guest.
7. After the join response is resolved, the user becomes joined.
8. If the user stays inactive for too long, the join becomes expired.

## MVP Scope

This is a simple version of join control.

Later versions can add more checks, but the MVP should avoid GPS entirely.

## Scope Of This File

This file goes deeper than workflows and voting rules.

It defines the join lifecycle for a visitor.
