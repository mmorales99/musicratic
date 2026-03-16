import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { Subscription, firstValueFrom } from "rxjs";
import { VoteService } from "@app/shared/services/vote.service";
import { VoteDirection } from "@app/shared/models/vote.model";
import { voteMachine, VoteMachineContext } from "./vote.machine";

@Injectable()
export class VoteMachineService implements OnDestroy {
  private readonly voteService = inject(VoteService);
  private readonly actor = createActor(voteMachine);
  private actorSub: XStateSubscription | null = null;
  private wsSub: Subscription | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<VoteMachineContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly currentVote = computed<VoteDirection | null>(
    () => this.ctx().currentVote,
  );
  readonly upCount = computed<number>(() => this.ctx().upCount);
  readonly downCount = computed<number>(() => this.ctx().downCount);
  readonly percentage = computed<number>(() => this.ctx().percentage);
  readonly error = computed<string | null>(() => this.ctx().error);
  readonly isVoting = computed(() => this.stateValue() === "voting");
  readonly isReady = computed(
    () =>
      this.stateValue() === "ready" ||
      this.stateValue() === "voting" ||
      this.stateValue() === "error",
  );
  readonly totalVoters = computed(
    () => this.ctx().upCount + this.ctx().downCount,
  );
  readonly downvotePercentage = computed<number>(() => {
    const total = this.totalVoters();
    return total === 0
      ? 0
      : Math.round((this.ctx().downCount / total) * 100);
  });
  readonly isNearSkipThreshold = computed(
    () => this.downvotePercentage() >= 50 && this.downvotePercentage() < 65,
  );
  readonly isAtSkipThreshold = computed(
    () => this.downvotePercentage() >= 65,
  );

  constructor() {
    this.actorSub = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  async init(hubId: string, entryId: string): Promise<void> {
    this.actor.send({ type: "INIT", hubId, entryId });
    this.subscribeToWsTally(entryId);

    try {
      const tally = await firstValueFrom(
        this.voteService.getTally(hubId, entryId),
      );
      this.actor.send({
        type: "TALLY_LOADED",
        upvotes: tally.upvotes,
        downvotes: tally.downvotes,
        currentVote: null, // Server doesn't return user's current vote in tally
      });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load vote tally";
      this.actor.send({ type: "VOTE_FAILED", error: message });
    }
  }

  async voteUp(): Promise<void> {
    const { hubId, entryId, currentVote } = this.ctx();
    if (!hubId || !entryId) return;

    if (currentVote === "up") {
      await this.removeVote();
      return;
    }

    this.actor.send({ type: "VOTE_UP" });
    try {
      await firstValueFrom(
        this.voteService.castVote(hubId, entryId, "up"),
      );
      this.actor.send({ type: "VOTE_SUCCESS" });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to cast vote";
      this.actor.send({ type: "VOTE_FAILED", error: message });
    }
  }

  async voteDown(): Promise<void> {
    const { hubId, entryId, currentVote } = this.ctx();
    if (!hubId || !entryId) return;

    if (currentVote === "down") {
      await this.removeVote();
      return;
    }

    this.actor.send({ type: "VOTE_DOWN" });
    try {
      await firstValueFrom(
        this.voteService.castVote(hubId, entryId, "down"),
      );
      this.actor.send({ type: "VOTE_SUCCESS" });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to cast vote";
      this.actor.send({ type: "VOTE_FAILED", error: message });
    }
  }

  async removeVote(): Promise<void> {
    const { hubId, entryId } = this.ctx();
    if (!hubId || !entryId) return;

    this.actor.send({ type: "REMOVE_VOTE" });
    try {
      await firstValueFrom(this.voteService.removeVote(hubId, entryId));
      this.actor.send({ type: "VOTE_SUCCESS" });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to remove vote";
      this.actor.send({ type: "VOTE_FAILED", error: message });
    }
  }

  reset(): void {
    this.unsubscribeWs();
    this.actor.send({ type: "RESET" });
  }

  private subscribeToWsTally(entryId: string): void {
    this.unsubscribeWs();
    this.wsSub = this.voteService.onTallyUpdated().subscribe((msg) => {
      if (msg.payload.entryId === entryId) {
        this.actor.send({
          type: "TALLY_UPDATED",
          upvotes: msg.payload.upvotes,
          downvotes: msg.payload.downvotes,
        });
      }
    });
  }

  private unsubscribeWs(): void {
    this.wsSub?.unsubscribe();
    this.wsSub = null;
  }

  ngOnDestroy(): void {
    this.reset();
    this.actorSub?.unsubscribe();
    this.actor.stop();
  }
}
