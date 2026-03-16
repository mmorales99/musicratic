import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { Subscription, firstValueFrom } from "rxjs";
import { QueueService } from "@app/shared/services/queue.service";
import { PlaybackService } from "@app/shared/services/playback.service";
import { queueMachine, QueueMachineContext } from "./queue.machine";
import { QueueEntry } from "@app/shared/models/track.model";
import { NowPlaying } from "@app/shared/models/playback.model";

@Injectable()
export class QueueMachineService implements OnDestroy {
  private readonly queueService = inject(QueueService);
  private readonly playbackService = inject(PlaybackService);
  private readonly actor = createActor(queueMachine);
  private actorSub: XStateSubscription | null = null;
  private wsSubs: Subscription[] = [];

  private readonly stateValue = signal<string>("disconnected");
  private readonly ctx = signal<QueueMachineContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly queue = computed<QueueEntry[]>(() => this.ctx().queue);
  readonly nowPlaying = computed<NowPlaying | null>(
    () => this.ctx().nowPlaying,
  );
  readonly error = computed<string | null>(() => this.ctx().error);
  readonly isConnected = computed(() => this.stateValue() === "connected");
  readonly isConnecting = computed(() => this.stateValue() === "connecting");
  readonly isEmpty = computed(
    () => this.ctx().queue.length === 0 && !this.ctx().nowPlaying,
  );

  constructor() {
    this.actorSub = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  async connect(hubId: string): Promise<void> {
    this.actor.send({ type: "CONNECT", hubId });

    try {
      this.queueService.connectToHub(hubId);
      this.subscribeToWsEvents();

      const [queuePage, nowPlaying] = await Promise.all([
        firstValueFrom(this.queueService.getQueue(hubId)),
        firstValueFrom(this.playbackService.getNowPlaying(hubId)),
      ]);

      this.actor.send({ type: "CONNECTED" });
      this.actor.send({ type: "QUEUE_LOADED", queue: queuePage.items });
      if (nowPlaying.entry) {
        this.actor.send({ type: "NOW_PLAYING", nowPlaying });
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to connect to queue";
      this.actor.send({ type: "CONNECTION_FAILED", error: message });
    }
  }

  disconnect(): void {
    this.unsubscribeWs();
    this.queueService.disconnectFromHub();
    this.actor.send({ type: "DISCONNECT" });
  }

  async retry(): Promise<void> {
    const hubId = this.ctx().hubId;
    if (!hubId) return;
    this.actor.send({ type: "RETRY" });
    await this.connect(hubId);
  }

  private subscribeToWsEvents(): void {
    this.unsubscribeWs();

    this.wsSubs.push(
      this.queueService.onQueueUpdated().subscribe((msg) => {
        this.actor.send({
          type: "QUEUE_UPDATED",
          queue: msg.payload.queue,
        });
      }),
    );

    this.wsSubs.push(
      this.queueService.onNowPlaying().subscribe((msg) => {
        this.actor.send({
          type: "NOW_PLAYING",
          nowPlaying: {
            hubId: msg.hubId,
            entry: msg.payload.entry,
            startedAt: msg.payload.startedAt,
            elapsedSeconds: 0,
            isPlaying: true,
          },
        });
      }),
    );

    this.wsSubs.push(
      this.queueService.onTrackEnded().subscribe((msg) => {
        this.actor.send({
          type: "TRACK_ENDED",
          entryId: msg.payload.entryId,
        });
      }),
    );

    this.wsSubs.push(
      this.queueService.onTrackSkipped().subscribe((msg) => {
        this.actor.send({
          type: "TRACK_SKIPPED",
          entryId: msg.payload.entryId,
          reason: msg.payload.reason,
        });
      }),
    );
  }

  private unsubscribeWs(): void {
    this.wsSubs.forEach((s) => s.unsubscribe());
    this.wsSubs = [];
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.actorSub?.unsubscribe();
    this.actor.stop();
  }
}
