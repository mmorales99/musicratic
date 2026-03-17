import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { RouterLink } from "@angular/router";
import { DatePipe, CurrencyPipe } from "@angular/common";
import { EconomyMachineService } from "../machines/economy-machine.service";
import { TransactionType } from "@app/shared/models/economy.model";

const FILTER_OPTIONS: { label: string; value: TransactionType | null }[] = [
  { label: "All", value: null },
  { label: "Credit", value: "credit" },
  { label: "Debit", value: "debit" },
  { label: "Refund", value: "refund" },
  { label: "Purchase", value: "purchase" },
  { label: "Reward", value: "reward" },
];

@Component({
  selector: "app-wallet",
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe],
  providers: [EconomyMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="wallet-page">
      @if (machine.isLoadingWallet()) {
        <div class="wallet-page__loading">Loading wallet...</div>
      } @else if (machine.error() && !machine.wallet(); as err) {
        <div class="wallet-page__error">
          <p>{{ err }}</p>
          <button class="btn btn--primary" (click)="retry()">Retry</button>
        </div>
      } @else if (machine.wallet(); as w) {
        <!-- Balance Card -->
        <div class="wallet-card">
          <div class="wallet-card__icon">🪙</div>
          <div class="wallet-card__balance">{{ w.balance }}</div>
          <div class="wallet-card__label">coins</div>
          <a routerLink="/wallet/purchase" class="btn btn--primary">
            Buy Coins
          </a>
        </div>

        <!-- Filter Chips -->
        <div class="filter-chips">
          @for (opt of filterOptions; track opt.value) {
            <button
              class="filter-chip"
              [class.filter-chip--active]="activeFilter() === opt.value"
              (click)="setFilter(opt.value)"
            >
              {{ opt.label }}
            </button>
          }
        </div>

        <!-- Transaction List -->
        @if (machine.isLoadingTransactions()) {
          <div class="wallet-page__loading">Loading transactions...</div>
        } @else if (machine.transactions().length === 0) {
          <div class="wallet-page__empty">
            <p>No transactions yet.</p>
          </div>
        } @else {
          <ul class="tx-list">
            @for (tx of machine.transactions(); track tx.id) {
              <li class="tx-item">
                <span class="tx-item__icon">{{ getTypeIcon(tx.type) }}</span>
                <div class="tx-item__info">
                  <span class="tx-item__reason">{{ tx.reason }}</span>
                  <span class="tx-item__date">
                    {{ tx.createdAt | date: "medium" }}
                  </span>
                </div>
                <span
                  class="tx-item__amount"
                  [class.tx-item__amount--positive]="tx.amount > 0"
                  [class.tx-item__amount--negative]="tx.amount < 0"
                >
                  {{ tx.amount > 0 ? "+" : "" }}{{ tx.amount }}
                </span>
              </li>
            }
          </ul>

          @if (machine.hasMoreTransactions()) {
            <button
              class="btn btn--secondary load-more"
              [disabled]="machine.isLoadingMore()"
              (click)="loadMore()"
            >
              {{ machine.isLoadingMore() ? "Loading..." : "Load More" }}
            </button>
          }
        }
      }
    </section>
  `,
  styles: [
    `
      .wallet-page {
        max-width: 600px;
        margin: 0 auto;
        padding: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }
      .wallet-page__loading,
      .wallet-page__error,
      .wallet-page__empty {
        text-align: center;
        padding: 2rem;
        color: #666;
      }
      .wallet-page__error p {
        color: #c0392b;
        margin-bottom: 1rem;
      }
      .wallet-card {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        padding: 2rem;
        border-radius: 16px;
        background: linear-gradient(135deg, #1a1a2e, #16213e);
        color: #fff;
      }
      .wallet-card__icon {
        font-size: 2.5rem;
      }
      .wallet-card__balance {
        font-size: 3rem;
        font-weight: 700;
        line-height: 1;
      }
      .wallet-card__label {
        font-size: 1rem;
        color: #aaa;
        margin-bottom: 0.5rem;
      }
      .filter-chips {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
      }
      .filter-chip {
        padding: 0.4rem 0.9rem;
        border-radius: 999px;
        border: 1px solid #ddd;
        background: #fff;
        cursor: pointer;
        font-size: 0.85rem;
        transition:
          background 0.2s,
          color 0.2s;
      }
      .filter-chip:hover {
        background: #f0f0f0;
      }
      .filter-chip--active {
        background: #1a1a2e;
        color: #fff;
        border-color: #1a1a2e;
      }
      .tx-list {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .tx-item {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        border-radius: 8px;
        background: #f8f9fa;
      }
      .tx-item:hover {
        background: #e9ecef;
      }
      .tx-item__icon {
        font-size: 1.5rem;
        flex-shrink: 0;
      }
      .tx-item__info {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .tx-item__reason {
        font-weight: 500;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .tx-item__date {
        font-size: 0.8rem;
        color: #888;
      }
      .tx-item__amount {
        font-weight: 600;
        font-size: 1rem;
        white-space: nowrap;
      }
      .tx-item__amount--positive {
        color: #27ae60;
      }
      .tx-item__amount--negative {
        color: #c0392b;
      }
      .load-more {
        align-self: center;
      }
      .btn {
        padding: 0.5rem 1.25rem;
        border-radius: 8px;
        border: none;
        cursor: pointer;
        font-weight: 500;
        font-size: 0.9rem;
        transition: opacity 0.2s;
      }
      .btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
      .btn--primary {
        background: #1db954;
        color: #fff;
      }
      .btn--secondary {
        background: #e9ecef;
        color: #333;
      }
    `,
  ],
})
export class WalletComponent implements OnInit {
  protected readonly machine = inject(EconomyMachineService);
  protected readonly filterOptions = FILTER_OPTIONS;
  protected readonly activeFilter = signal<TransactionType | null>(null);

  ngOnInit(): void {
    void this.machine.loadWallet();
    void this.machine.loadTransactions();
  }

  protected setFilter(filter: TransactionType | null): void {
    this.activeFilter.set(filter);
    void this.machine.loadTransactions(filter);
  }

  protected loadMore(): void {
    void this.machine.loadMoreTransactions();
  }

  protected retry(): void {
    this.machine.retry();
    void this.machine.loadWallet();
    void this.machine.loadTransactions();
  }

  protected getTypeIcon(type: string): string {
    switch (type) {
      case "credit":
        return "💰";
      case "debit":
        return "💸";
      case "refund":
        return "↩️";
      case "purchase":
        return "🛒";
      case "reward":
        return "🎁";
      default:
        return "📝";
    }
  }
}
