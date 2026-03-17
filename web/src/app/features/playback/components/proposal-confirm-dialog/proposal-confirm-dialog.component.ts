import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
} from "@angular/core";
import { TrackSearchResult } from "@app/shared/models/playback.model";
import { TrackPrice, Wallet } from "@app/shared/models/economy.model";

@Component({
  selector: "app-proposal-confirm-dialog",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dialog-backdrop" (click)="cancel.emit()">
      <div class="dialog" (click)="$event.stopPropagation()">
        <h3 class="dialog__title">Confirm Proposal</h3>

        <!-- Track info -->
        <div class="dialog__track">
          <div class="dialog__cover">
            @if (track().coverUrl) {
              <img
                [src]="track().coverUrl"
                [alt]="track().title"
                class="dialog__cover-img"
              />
            } @else {
              <div class="dialog__cover-placeholder">♪</div>
            }
          </div>
          <div class="dialog__info">
            <span class="dialog__track-title">{{ track().title }}</span>
            <span class="dialog__artist">{{ track().artist }}</span>
          </div>
        </div>

        <!-- Cost details -->
        @if (price()) {
          <div class="dialog__pricing">
            <div class="dialog__row">
              <span>Cost</span>
              <span class="dialog__coins">
                {{ price()!.finalCost }} coins
              </span>
            </div>
            @if (price()!.hotnessMultiplier > 1) {
              <div class="dialog__row dialog__row--detail">
                <span>Hotness multiplier</span>
                <span>×{{ price()!.hotnessMultiplier }}</span>
              </div>
            }
            @if (wallet()) {
              <div class="dialog__row">
                <span>Your balance</span>
                <span
                  class="dialog__balance"
                  [class.dialog__balance--low]="!hasEnough()"
                >
                  {{ wallet()!.balance }} coins
                </span>
              </div>
            }
          </div>
        }

        @if (!hasEnough()) {
          <div class="dialog__warning">
            Insufficient coins. You need
            {{ (price()?.finalCost ?? 0) - (wallet()?.balance ?? 0) }}
            more coins.
          </div>
        }

        <!-- Actions -->
        <div class="dialog__actions">
          <button class="btn btn--secondary" (click)="cancel.emit()">
            Cancel
          </button>
          <button
            class="btn btn--primary"
            [disabled]="!hasEnough()"
            (click)="confirm.emit()"
          >
            Propose ({{ price()?.finalCost ?? 0 }} coins)
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .dialog-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
      }
      .dialog {
        background: #fff;
        border-radius: 12px;
        padding: 1.5rem;
        max-width: 400px;
        width: 90%;
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .dialog__title {
        font-size: 1.25rem;
        font-weight: 600;
        margin: 0;
      }
      .dialog__track {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        background: #f8f9fa;
        border-radius: 8px;
      }
      .dialog__cover { width: 56px; height: 56px; flex-shrink: 0; }
      .dialog__cover-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 4px;
      }
      .dialog__cover-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #ddd;
        border-radius: 4px;
        font-size: 1.5rem;
      }
      .dialog__info {
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .dialog__track-title {
        font-weight: 500;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .dialog__artist { font-size: 0.85rem; color: #666; }
      .dialog__pricing {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .dialog__row {
        display: flex;
        justify-content: space-between;
        font-size: 0.95rem;
      }
      .dialog__row--detail {
        font-size: 0.85rem;
        color: #888;
      }
      .dialog__coins { font-weight: 600; color: #f59e0b; }
      .dialog__balance { font-weight: 500; }
      .dialog__balance--low { color: #dc2626; }
      .dialog__warning {
        padding: 0.5rem 0.75rem;
        background: #fef2f2;
        color: #dc2626;
        border-radius: 6px;
        font-size: 0.85rem;
        text-align: center;
      }
      .dialog__actions {
        display: flex;
        gap: 0.75rem;
        justify-content: flex-end;
      }
    `,
  ],
})
export class ProposalConfirmDialogComponent {
  readonly track = input.required<TrackSearchResult>();
  readonly price = input<TrackPrice | null>(null);
  readonly wallet = input<Wallet | null>(null);

  readonly confirm = output<void>();
  readonly cancel = output<void>();

  protected hasEnough(): boolean {
    const p = this.price();
    const w = this.wallet();
    if (!p || !w) return false;
    return w.balance >= p.finalCost;
  }
}
