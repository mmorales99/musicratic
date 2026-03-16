import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { firstValueFrom } from "rxjs";
import { ListService } from "@app/shared/services/list.service";
import { listDetailMachine, ListDetailContext } from "./list-detail.machine";
import {
  HubList,
  ListTrack,
  AddTrackRequest,
  BulkAddTracksRequest,
  PlayMode,
} from "@app/shared/models/hub.model";

@Injectable()
export class ListDetailMachineService implements OnDestroy {
  private readonly listService = inject(ListService);
  private readonly actor = createActor(listDetailMachine);
  private subscription: XStateSubscription | null = null;
  private hubId: string | null = null;
  private listId: string | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<ListDetailContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly list = computed<HubList | null>(() => this.ctx().list);
  readonly tracks = computed<ListTrack[]>(() => this.ctx().tracks);
  readonly error = computed<string | null>(() => this.ctx().error);
  readonly isLoading = computed<boolean>(() => this.stateValue() === "loading");

  constructor() {
    this.subscription = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  async load(hubId: string, listId: string): Promise<void> {
    this.hubId = hubId;
    this.listId = listId;
    this.actor.send({ type: "LOAD", hubId, listId });

    try {
      const [listsEnvelope, tracksEnvelope] = await Promise.all([
        firstValueFrom(this.listService.getLists(hubId)),
        firstValueFrom(this.listService.getTracks(hubId, listId)),
      ]);
      const list = listsEnvelope.items.find((l) => l.id === listId);
      if (!list) {
        this.actor.send({ type: "LOAD_FAILED", error: "List not found" });
        return;
      }
      this.actor.send({ type: "LOADED", list, tracks: tracksEnvelope.items });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load list";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  async addTrack(trackData: AddTrackRequest): Promise<void> {
    if (!this.hubId || !this.listId) return;
    try {
      const track = await firstValueFrom(
        this.listService.addTrack(this.hubId, this.listId, trackData),
      );
      this.actor.send({ type: "TRACK_ADDED", track });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to add track";
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  async removeTrack(trackId: string): Promise<void> {
    if (!this.hubId || !this.listId) return;
    try {
      await firstValueFrom(
        this.listService.removeTrack(this.hubId, this.listId, trackId),
      );
      this.actor.send({ type: "TRACK_REMOVED", trackId });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to remove track";
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  async reorderTrack(trackId: string, newPosition: number): Promise<void> {
    if (!this.hubId || !this.listId) return;
    try {
      await firstValueFrom(
        this.listService.reorderTrack(
          this.hubId,
          this.listId,
          trackId,
          newPosition,
        ),
      );
      const reordered = [...this.tracks()].sort((a, b) => {
        if (a.id === trackId) return newPosition - b.position;
        if (b.id === trackId) return a.position - newPosition;
        return a.position - b.position;
      });
      this.actor.send({ type: "TRACKS_REORDERED", tracks: reordered });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to reorder track";
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  async bulkAdd(tracks: BulkAddTracksRequest): Promise<void> {
    if (!this.hubId || !this.listId) return;
    try {
      await firstValueFrom(
        this.listService.bulkAddTracks(this.hubId, this.listId, tracks),
      );
      this.reload();
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to bulk add tracks";
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  async setPlayMode(mode: PlayMode): Promise<void> {
    if (!this.hubId || !this.listId) return;
    try {
      const list = await firstValueFrom(
        this.listService.setPlayMode(this.hubId, this.listId, mode),
      );
      this.actor.send({ type: "PLAY_MODE_CHANGED", list });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to change play mode";
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  clearError(): void {
    this.actor.send({ type: "CLEAR_ERROR" });
  }

  retry(): void {
    if (this.hubId && this.listId) {
      this.load(this.hubId, this.listId);
    }
  }

  private reload(): void {
    if (this.hubId && this.listId) {
      this.load(this.hubId, this.listId);
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.actor.stop();
  }
}
