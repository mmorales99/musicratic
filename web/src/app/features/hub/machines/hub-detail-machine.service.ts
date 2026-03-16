import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { firstValueFrom } from "rxjs";
import { HubService } from "@app/shared/services/hub.service";
import { ListService } from "@app/shared/services/list.service";
import { hubDetailMachine, HubDetailContext } from "./hub-detail.machine";
import {
  Hub,
  HubSettings,
  HubList,
  UpdateHubRequest,
} from "@app/shared/models/hub.model";

@Injectable()
export class HubDetailMachineService implements OnDestroy {
  private readonly hubService = inject(HubService);
  private readonly listService = inject(ListService);
  private readonly actor = createActor(hubDetailMachine);
  private subscription: XStateSubscription | null = null;
  private currentHubId: string | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<HubDetailContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly hub = computed<Hub | null>(() => this.ctx().hub);
  readonly settings = computed<HubSettings | null>(() => this.ctx().settings);
  readonly lists = computed<HubList[]>(() => this.ctx().lists);
  readonly error = computed<string | null>(() => this.ctx().error);
  readonly isLoading = computed<boolean>(() => this.stateValue() === "loading");
  readonly isSaving = computed<boolean>(() =>
    ["saving", "savingSettings", "executingAction"].includes(this.stateValue()),
  );
  readonly isEditing = computed<boolean>(() => this.stateValue() === "editing");

  constructor() {
    this.subscription = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  async load(hubId: string): Promise<void> {
    this.currentHubId = hubId;
    this.actor.send({ type: "LOAD", hubId });

    try {
      const [hub, settings, listsEnvelope] = await Promise.all([
        firstValueFrom(this.hubService.getHub(hubId)),
        firstValueFrom(this.hubService.getHubSettings(hubId)),
        firstValueFrom(this.listService.getLists(hubId)),
      ]);
      this.actor.send({
        type: "LOADED",
        hub,
        settings,
        lists: listsEnvelope.items,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to load hub";
      this.actor.send({ type: "LOAD_FAILED", error: message });
    }
  }

  edit(): void {
    this.actor.send({ type: "EDIT" });
  }

  cancelEdit(): void {
    this.actor.send({ type: "CANCEL_EDIT" });
  }

  async save(data: UpdateHubRequest): Promise<void> {
    if (!this.currentHubId) return;
    this.actor.send({ type: "SAVE", hub: data as Partial<Hub> });

    try {
      const hub = await firstValueFrom(
        this.hubService.updateHub(this.currentHubId, data),
      );
      this.actor.send({ type: "SAVED", hub });
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to save hub";
      this.actor.send({ type: "SAVE_FAILED", error: message });
    }
  }

  async saveSettings(data: Partial<HubSettings>): Promise<void> {
    if (!this.currentHubId) return;
    this.actor.send({ type: "SAVE_SETTINGS", settings: data });

    try {
      const settings = await firstValueFrom(
        this.hubService.updateHubSettings(this.currentHubId, data),
      );
      this.actor.send({ type: "SETTINGS_SAVED", settings });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to save settings";
      this.actor.send({ type: "SAVE_FAILED", error: message });
    }
  }

  async executeAction(
    action: "activate" | "deactivate" | "pause" | "resume" | "delete",
  ): Promise<void> {
    if (!this.currentHubId) return;
    this.actor.send({ type: "ACTION", action });

    try {
      if (action === "delete") {
        await firstValueFrom(this.hubService.deleteHub(this.currentHubId));
        this.actor.send({ type: "DELETED" });
        return;
      }

      const actionMap = {
        activate: () => this.hubService.activateHub(this.currentHubId!),
        deactivate: () => this.hubService.deactivateHub(this.currentHubId!),
        pause: () => this.hubService.pauseHub(this.currentHubId!),
        resume: () => this.hubService.resumeHub(this.currentHubId!),
      } as const;

      const hub = await firstValueFrom(actionMap[action]());
      this.actor.send({ type: "ACTION_DONE", hub });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : `Failed to ${action} hub`;
      this.actor.send({ type: "ACTION_FAILED", error: message });
    }
  }

  retry(): void {
    if (this.currentHubId) {
      this.load(this.currentHubId);
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.actor.stop();
  }
}
