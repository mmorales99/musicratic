import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute, RouterLink } from "@angular/router";
import { CurrencyPipe } from "@angular/common";
import { EconomyMachineService } from "../machines/economy-machine.service";
import { ToastService } from "@app/shared/services/toast.service";

@Component({
  selector: "app-coin-purchase",
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  providers: [EconomyMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="purchase-page">
      <div class="purchase-page__header">
        <a routerLink="/wallet" class="purchase-page__back">← Back to Wallet</a>
        <h1>Buy Coins</h1>
        <p>Choose a coin package to boost your music influence.</p>
      </div>

      @if (purchaseResult(); as result) {
        <div
          class="purchase-banner"
          [class.purchase-banner--success]="result === 'success'"
          [class.purchase-banner--cancel]="result === 'cancel'"
        >
          @if (result === "success") {
            <p>🎉 Purchase completed! Coins have been added to your wallet.</p>
          } @else {
            <p>Purchase was cancelled. No charges were made.</p>
          }
          <button class="btn btn--secondary btn--small" (click)="dismissResult()">
            Dismiss
          </button>
        </div>
      }

      @if (machine.isLoadingPackages()) {
        <div class="purchase-page__loading">Loading coin packages...</div>
      } @else if (machine.error(); as err) {
        <div class="purchase-page__error">
          <p>{{ err }}</p>
          <button class="btn btn--primary" (click)="reload()">Retry</button>
        </div>
      } @else {
        <div class="package-grid">
          @for (pkg of machine.coinPackages(); track pkg.id) {
            <div class="package-card">
              @if (pkg.bonusCoins > 0) {
                <div class="package-card__badge">
                  +{{ pkg.bonusCoins }} bonus
                </div>
              }
              <h3 class="package-card__name">{{ pkg.name }}</h3>
              <div class="package-card__coins">
                <span class="package-card__amount">{{ pkg.coinAmount }}</span>
                <span class="package-card__unit">coins</span>
              </div>
              @if (pkg.bonusCoins > 0) {
                <div class="package-card__total">
                  {{ pkg.coinAmount + pkg.bonusCoins }} total
                </div>
              }
              <div class="package-card__price">
                {{ pkg.priceUsd | currency: "USD" }}
              </div>
              <button
                class="btn btn--primary package-card__buy"
                [disabled]="machine.isPurchasing()"
                (click)="buy(pkg.id)"
              >
                {{ machine.isPurchasing() ? "Processing..." : "Buy" }}
              </button>
            </div>
          }
        </div>
      }
    </section>
  `,
  styles: [
    `
      .purchase-page {
        max-width: 800px;
        margin: 0 auto;
        padding: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }
      .purchase-page__header {
        text-align: center;
      }
      .purchase-page__header h1 {
        margin: 0.5rem 0 0.25rem;
      }
      .purchase-page__header p {
        color: #666;
        margin: 0;
      }
      .purchase-page__back {
        display: inline-block;
        margin-bottom: 0.5rem;
        color: #1db954;
        text-decoration: none;
        font-size: 0.9rem;
      }
      .purchase-page__back:hover {
        text-decoration: underline;
      }
      .purchase-page__loading,
      .purchase-page__error {
        text-align: center;
        padding: 2rem;
        color: #666;
      }
      .purchase-page__error p {
        color: #c0392b;
        margin-bottom: 1rem;
      }
      .purchase-banner {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        padding: 1rem 1.25rem;
        border-radius: 8px;
      }
      .purchase-banner p {
        margin: 0;
      }
      .purchase-banner--success {
        background: #d4edda;
        color: #155724;
      }
      .purchase-banner--cancel {
        background: #fff3cd;
        color: #856404;
      }
      .package-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 1rem;
      }
      .package-card {
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        padding: 1.5rem 1rem;
        border-radius: 12px;
        background: #f8f9fa;
        border: 2px solid transparent;
        transition:
          border-color 0.2s,
          transform 0.2s;
      }
      .package-card:hover {
        border-color: #1db954;
        transform: translateY(-2px);
      }
      .package-card__badge {
        position: absolute;
        top: -0.5rem;
        right: -0.5rem;
        padding: 0.2rem 0.6rem;
        border-radius: 999px;
        background: #f39c12;
        color: #fff;
        font-size: 0.75rem;
        font-weight: 600;
      }
      .package-card__name {
        margin: 0;
        font-size: 1rem;
        color: #666;
      }
      .package-card__coins {
        display: flex;
        align-items: baseline;
        gap: 0.25rem;
      }
      .package-card__amount {
        font-size: 2rem;
        font-weight: 700;
        color: #1a1a2e;
      }
      .package-card__unit {
        font-size: 0.9rem;
        color: #888;
      }
      .package-card__total {
        font-size: 0.8rem;
        color: #27ae60;
        font-weight: 500;
      }
      .package-card__price {
        font-size: 1.1rem;
        font-weight: 600;
        color: #333;
      }
      .package-card__buy {
        width: 100%;
        margin-top: 0.25rem;
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
      .btn--small {
        padding: 0.3rem 0.8rem;
        font-size: 0.8rem;
      }
    `,
  ],
})
export class CoinPurchaseComponent implements OnInit {
  protected readonly machine = inject(EconomyMachineService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly purchaseResult = signal<"success" | "cancel" | null>(null);

  ngOnInit(): void {
    this.handleStripeRedirect();
    void this.machine.loadPackages();
  }

  protected buy(packageId: string): void {
    void this.machine.purchase(packageId);
  }

  protected reload(): void {
    this.machine.retry();
    void this.machine.loadPackages();
  }

  protected dismissResult(): void {
    this.purchaseResult.set(null);
  }

  private handleStripeRedirect(): void {
    const params = this.route.snapshot.queryParamMap;
    if (params.get("success") === "true") {
      this.purchaseResult.set("success");
      this.toast.show("success", "Coins purchased successfully!");
    } else if (params.get("canceled") === "true") {
      this.purchaseResult.set("cancel");
    }
  }
}
