# Master Sprint Backlog

This file flattens the backlog into one implementation order.

`★` marks the best first tasks.

1. ★ [Define payload schema](subtasks/05-session-details/01-define-payload-schema.md) - Feature: Session payload schema - Depends on: none
2. ★ [Define join states](subtasks/01-join-states/01-define-join-states.md) - Feature: Join state model - Depends on: none
3. ★ [Define dead state](subtasks/02-active-session-states/01-define-dead-state.md) - Feature: Dead session state - Depends on: none
4. ★ [Detect app authentication](subtasks/03-auth-flow/01-detect-app-authentication.md) - Feature: Authentication detection - Depends on: none
5. ★ [Define timeout-triggering actions](subtasks/11-activity-timeout/01-define-timeout-triggering-actions.md) - Feature: Timeout-triggering actions - Depends on: none
6. ★ [Record up and down votes](subtasks/07-vote-counting/01-record-up-and-down-votes.md) - Feature: Vote recording - Depends on: none
7. ★ [Store tie-break mode](subtasks/08-tie-break-rules/01-store-tie-break-mode.md) - Feature: Tie-break mode storage - Depends on: none
8. [Define live state](subtasks/02-active-session-states/02-define-live-state.md) - Feature: Live session state - Depends on: define dead state
9. [Implement join transitions](subtasks/01-join-states/02-implement-join-transitions.md) - Feature: Join transitions - Depends on: define join states, detect app authentication, define payload schema
10. [Send suggestion permission flag](subtasks/03-auth-flow/02-send-suggestion-permission-flag.md) - Feature: Suggestion permission flag - Depends on: detect app authentication
11. [Include music session data](subtasks/05-session-details/02-include-music-session-data.md) - Feature: Music session data - Depends on: define payload schema
12. [Include vote and code data](subtasks/05-session-details/03-include-vote-and-code-data.md) - Feature: Vote and session code data - Depends on: define payload schema
13. [Implement play and close transitions](subtasks/02-active-session-states/03-implement-play-close-transitions.md) - Feature: Session state transitions - Depends on: define dead state, define live state
14. [Select a song from the server list](subtasks/04-song-suggestion-flow/01-select-song.md) - Feature: Song selection - Depends on: include music session data, send suggestion permission flag
15. [Enqueue suggestion](subtasks/04-song-suggestion-flow/02-enqueue-suggestion.md) - Feature: FIFO suggestion queue - Depends on: select a song from the server list
16. [Keep pending for voting](subtasks/04-song-suggestion-flow/03-keep-pending-for-voting.md) - Feature: Pending voting availability - Depends on: enqueue suggestion
17. [Count the winning option](subtasks/07-vote-counting/02-count-the-winning-option.md) - Feature: Vote winner counting - Depends on: record up and down votes
18. [Keep non-tied results deterministic](subtasks/07-vote-counting/03-keep-non-tied-results-deterministic.md) - Feature: Deterministic vote results - Depends on: count the winning option
19. [Resolve ties with vote priority](subtasks/08-tie-break-rules/02-resolve-ties-with-vote-priority.md) - Feature: Vote-priority tie resolution - Depends on: count the winning option, store tie-break mode
20. [Require shop owner tie resolution](subtasks/08-tie-break-rules/03-require-shop-owner-tie-resolution.md) - Feature: Owner tie resolution - Depends on: store tie-break mode
21. [Detect promotion threshold](subtasks/06-queue-promotion/01-detect-promotion-threshold.md) - Feature: Promotion threshold detection - Depends on: implement play and close transitions
22. [Promote top pending song](subtasks/06-queue-promotion/02-promote-top-pending-song.md) - Feature: Top pending song promotion - Depends on: detect promotion threshold, enqueue suggestion
23. [Preserve queue order after promotion](subtasks/06-queue-promotion/03-preserve-queue-order-after-promotion.md) - Feature: Queue order preservation - Depends on: promote top pending song
24. [Select the next song](subtasks/10-playback-transition/01-select-the-next-song.md) - Feature: Next song selection - Depends on: promote top pending song, preserve queue order after promotion
25. [Remove suggested songs on start](subtasks/10-playback-transition/02-remove-suggested-songs-on-start.md) - Feature: Suggested song queue cleanup - Depends on: select the next song
26. [Advance playback state](subtasks/10-playback-transition/03-advance-playback-state.md) - Feature: Playback state advance - Depends on: select the next song, define live state
27. [Remove a pending song](subtasks/09-owner-override-rules/01-remove-a-pending-song.md) - Feature: Pending song removal - Depends on: enqueue suggestion
28. [Skip a playing song](subtasks/09-owner-override-rules/02-skip-a-playing-song.md) - Feature: Playing song skip - Depends on: advance playback state
29. [Shift the queue after override](subtasks/09-owner-override-rules/03-shift-the-queue-after-override.md) - Feature: Queue shift after override - Depends on: remove a pending song, skip a playing song
30. [Reset the inactivity timer](subtasks/11-activity-timeout/02-reset-the-inactivity-timer.md) - Feature: Inactivity timer reset - Depends on: define timeout-triggering actions
31. [Disconnect after one hour](subtasks/11-activity-timeout/03-disconnect-after-one-hour.md) - Feature: Inactivity disconnect - Depends on: reset the inactivity timer
32. [Enforce the reconnect wait](subtasks/12-reconnect-rules/01-enforce-the-reconnect-wait.md) - Feature: Reconnect wait rule - Depends on: disconnect after one hour
33. [Restore the base state](subtasks/12-reconnect-rules/02-restore-the-base-state.md) - Feature: Base state restoration - Depends on: detect app authentication
34. [Restore auth state on reconnect](subtasks/03-auth-flow/03-restore-auth-state-on-reconnect.md) - Feature: Auth state restoration - Depends on: detect app authentication, enforce the reconnect wait
35. [Reuse the first join flow](subtasks/12-reconnect-rules/03-reuse-the-first-join-flow.md) - Feature: Reconnect join flow reuse - Depends on: include music session data, restore auth state on reconnect
36. [Handle expiration and rejoin](subtasks/01-join-states/03-handle-expiration-rejoin.md) - Feature: Join expiration and rejoin - Depends on: disconnect after one hour, enforce the reconnect wait, restore auth state on reconnect
