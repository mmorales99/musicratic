# Documentation Map

This folder is organized from the most abstract view to the most concrete one.

1. `00-product-description.md` explains what the finished product is.
2. `01-main-actors.md` identifies who interacts with the product.
3. `02-actor-structure.md` breaks those actors into more specific responsibilities and relationships.
4. `03-core-workflows.md` describes the main user flows for the MVP.
5. `04-vote-rules.md` defines how session voting and authority work.
6. `05-session-lifecycle.md` defines the visitor join states.
7. `06-active-session-states.md` defines the session runtime states.
8. `07-expiration-reconnect-rules.md` defines inactivity timeout and reconnect rules.
9. `08-auth-and-suggestion-rules.md` defines who can suggest songs in each join state.
10. `09-activity-and-timeout-rules.md` defines which actions refresh the timeout.
11. `10-reconnect-rules.md` defines what happens after inactivity disconnects.
12. `11-auth-flow.md` defines how app-level authentication affects the join.
13. `12-song-suggestion-flow.md` defines how a song suggestion enters the queue.
14. `13-session-details.md` defines the data sent when a visitor joins.
15. `14-queue-promotion.md` defines when the next candidate is selected.
16. `15-vote-result-rules.md` defines how vote results affect playback.
17. `16-owner-override-rules.md` defines what the owner can override.
18. `17-playback-transition.md` defines how a candidate song starts playing.
19. `18-tie-break-rules.md` defines how tied votes are resolved.

The rule is simple:

- start with the product as a whole
- then describe the actors
- then descend into the internal structure of each actor
- then describe the main workflows that connect those actors
- then define state models and lifecycle rules
- then define permissions, activity, and reconnect behavior
- then define auth, suggestion, and payload details
- then define queue promotion, vote counting, tie breaks, owner overrides, and playback transitions
- keep each file one abstraction level deeper than the previous one
