import { Injectable, inject, signal, computed, OnDestroy } from "@angular/core";
import { createActor, Subscription as XStateSubscription } from "xstate";
import { firstValueFrom } from "rxjs";
import { HubService } from "@app/shared/services/hub.service";
import { hubJoinMachine, HubJoinContext } from "./hub-join.machine";
import { HubAttachment } from "@app/shared/models/hub.model";

@Injectable()
export class HubJoinMachineService implements OnDestroy {
  private readonly hubService = inject(HubService);
  private readonly actor = createActor(hubJoinMachine);
  private subscription: XStateSubscription | null = null;

  private readonly stateValue = signal<string>("idle");
  private readonly ctx = signal<HubJoinContext>(
    this.actor.getSnapshot().context,
  );

  readonly state = this.stateValue.asReadonly();
  readonly code = computed<string>(() => this.ctx().code);
  readonly attachment = computed<HubAttachment | null>(
    () => this.ctx().attachment,
  );
  readonly error = computed<string | null>(() => this.ctx().error);
  readonly isAttaching = computed<boolean>(
    () => this.stateValue() === "attaching",
  );
  readonly isAttached = computed<boolean>(
    () => this.stateValue() === "attached",
  );

  constructor() {
    this.subscription = this.actor.subscribe((snapshot) => {
      this.stateValue.set(snapshot.value as string);
      this.ctx.set(snapshot.context);
    });
    this.actor.start();
  }

  setCode(code: string): void {
    this.actor.send({ type: "SET_CODE", code });
  }

  async submit(): Promise<void> {
    const code = this.ctx().code.trim();
    if (!code) return;

    this.actor.send({ type: "SUBMIT" });

    try {
      const attachment = await firstValueFrom(
        this.hubService.attachToHub(code),
      );
      this.actor.send({ type: "ATTACHED", attachment });
    } catch (err) {
      const message = this.parseError(err);
      this.actor.send({ type: "ATTACH_FAILED", error: message });
    }
  }

  retry(): void {
    this.submit();
  }

  reset(): void {
    this.actor.send({ type: "RESET" });
  }

  private parseError(err: unknown): string {
    if (err && typeof err === "object" && "error" in err) {
      const httpErr = err as {
        error?: { detail?: string; title?: string };
        status?: number;
      };
      if (httpErr.status === 404)
        return "Hub not found. Check the code and try again.";
      if (httpErr.status === 409)
        return "You are already attached to this hub.";
      if (httpErr.status === 422) return "This hub is not currently active.";
      return (
        httpErr.error?.detail ?? httpErr.error?.title ?? "Failed to join hub."
      );
    }
    return err instanceof Error ? err.message : "Failed to join hub.";
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.actor.stop();
  }
}
