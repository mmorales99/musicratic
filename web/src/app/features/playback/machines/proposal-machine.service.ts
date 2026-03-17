import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { TrackService } from "@app/shared/services/track.service";
import { EconomyService } from "@app/shared/services/economy.service";
import { ProposalService } from "../services/proposal.service";
import {
  proposalMachine,
  ProposalMachineContext,
} from "./proposal.machine";
import { TrackSearchResult } from "@app/shared/models/playback.model";
import { MusicProvider } from "@app/shared/models/hub.model";

@Injectable()
export class ProposalMachineService implements OnDestroy {
  private readonly trackService = inject(TrackService);
  private readonly economyService = inject(EconomyService);
  private readonly proposalService = inject(ProposalService);
  private readonly actor = createActor(proposalMachine);
  private actorSub: XStateSubscription | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<ProposalMachineContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly searchResults = computed(() => this.ctx().searchResults);
  readonly selectedTrack = computed(() => this.ctx().selectedTrack);
  readonly trackPrice = computed(() => this.ctx().trackPrice);
  readonly wallet = computed(() => this.ctx().wallet);
  readonly error = computed(() => this.ctx().error);
  readonly isSearching = computed(() => this.stateValue() === "searching");
  readonly isConfirming = computed(() => this.stateValue() === "confirming");
  readonly isProposing = computed(() => this.stateValue() === "proposing");
  readonly isSuccess = computed(() => this.stateValue() === "success");
  readonly isError = computed(() => this.stateValue() === "error");

  constructor() {
    this.actorSub = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  setHub(hubId: string): void {
    this.actor.send({ type: "SET_HUB", hubId });
  }

  async search(query: string, provider?: MusicProvider): Promise<void> {
    this.actor.send({ type: "SEARCH", query });
    try {
      const response = await firstValueFrom(
        this.trackService.searchTracks(query, provider),
      );
      this.actor.send({ type: "SEARCH_SUCCESS", results: response.results });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Search failed";
      this.actor.send({ type: "SEARCH_FAILED", error: message });
    }
  }

  async selectTrack(track: TrackSearchResult): Promise<void> {
    this.actor.send({ type: "SELECT_TRACK", track });
    try {
      const [price, wallet] = await Promise.all([
        firstValueFrom(
          this.economyService.getTrackPrice(
            track.durationSeconds,
            0, // hotnessScore not available on search result
          ),
        ),
        firstValueFrom(this.economyService.getWallet()),
      ]);
      this.actor.send({ type: "PRICE_LOADED", price, wallet });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load pricing";
      this.actor.send({ type: "PRICE_FAILED", error: message });
    }
  }

  async confirm(): Promise<void> {
    this.actor.send({ type: "CONFIRM" });
    await this.doPropose();
  }

  cancel(): void {
    this.actor.send({ type: "CANCEL" });
  }

  reset(): void {
    this.actor.send({ type: "RESET" });
  }

  private async doPropose(): Promise<void> {
    const ctx = this.ctx();
    if (!ctx.hubId || !ctx.selectedTrack) return;

    try {
      const result = await firstValueFrom(
        this.proposalService.proposeTrack(
          ctx.hubId,
          ctx.selectedTrack.id,
          ctx.selectedTrack.provider,
        ),
      );
      this.actor.send({ type: "PROPOSE_SUCCESS", entryId: result.entryId });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Proposal failed";
      this.actor.send({ type: "PROPOSE_FAILED", error: message });
    }
  }

  ngOnDestroy(): void {
    this.actorSub?.unsubscribe();
    this.actor.stop();
  }
}
