import { Component, ChangeDetectionStrategy, inject } from "@angular/core";
import { ToastService, Toast } from "@app/shared/services/toast.service";

@Component({
  selector: "app-toast-container",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (toastService.hasToasts()) {
      <div class="toast-container" role="status" aria-live="polite">
        @for (toast of toastService.toasts(); track toast.id) {
          <div
            class="toast toast--{{ toast.type }}"
            (click)="toastService.dismiss(toast.id)"
          >
            <span class="toast__icon">{{ iconFor(toast.type) }}</span>
            <div class="toast__body">
              <span class="toast__message">{{ toast.message }}</span>
              @if (toast.detail) {
                <span class="toast__detail">{{ toast.detail }}</span>
              }
            </div>
            <button
              class="toast__close"
              (click)="$event.stopPropagation(); toastService.dismiss(toast.id)"
              aria-label="Dismiss"
            >
              ×
            </button>
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      .toast-container {
        position: fixed;
        top: 1rem;
        right: 1rem;
        z-index: 9999;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        max-width: 400px;
      }
      .toast {
        display: flex;
        align-items: flex-start;
        gap: 0.75rem;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        color: #fff;
        cursor: pointer;
        animation: slideIn 0.3s ease-out;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
      }
      .toast--warning {
        background: #e67e22;
      }
      .toast--info {
        background: #3498db;
      }
      .toast--neutral {
        background: #636e72;
      }
      .toast--success {
        background: #27ae60;
      }
      .toast--error {
        background: #c0392b;
      }
      .toast__icon {
        font-size: 1.25rem;
        flex-shrink: 0;
      }
      .toast__body {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 0.2rem;
      }
      .toast__message {
        font-weight: 600;
        font-size: 0.9rem;
      }
      .toast__detail {
        font-size: 0.8rem;
        opacity: 0.9;
      }
      .toast__close {
        background: none;
        border: none;
        color: #fff;
        font-size: 1.25rem;
        cursor: pointer;
        padding: 0;
        line-height: 1;
        opacity: 0.7;
      }
      .toast__close:hover {
        opacity: 1;
      }
      @keyframes slideIn {
        from {
          transform: translateX(100%);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }
    `,
  ],
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);

  protected iconFor(type: string): string {
    const icons: Record<string, string> = {
      warning: "⚠️",
      info: "ℹ️",
      neutral: "➡️",
      success: "✅",
      error: "❌",
    };
    return icons[type] ?? "ℹ️";
  }
}
