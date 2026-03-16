import { Injectable, signal, computed } from "@angular/core";

export type ToastType = "warning" | "info" | "neutral" | "success" | "error";

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
  detail: string | null;
  duration: number;
}

const DEFAULT_DURATION = 5000;
let nextId = 1;

@Injectable({ providedIn: "root" })
export class ToastService {
  private readonly toastsSignal = signal<Toast[]>([]);

  readonly toasts = this.toastsSignal.asReadonly();
  readonly hasToasts = computed(() => this.toastsSignal().length > 0);

  show(
    type: ToastType,
    message: string,
    detail: string | null = null,
    duration: number = DEFAULT_DURATION,
  ): number {
    const id = nextId++;
    const toast: Toast = { id, type, message, detail, duration };

    this.toastsSignal.update((list) => [...list, toast]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }

    return id;
  }

  dismiss(id: number): void {
    this.toastsSignal.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    this.toastsSignal.set([]);
  }
}
