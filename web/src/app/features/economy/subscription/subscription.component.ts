import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
  computed,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { DatePipe } from "@angular/common";
import { firstValueFrom } from "rxjs";
import { EconomyService } from "@app/shared/services/economy.service";
import { ToastService } from "@app/shared/services/toast.service";
import {
  Subscription,
  SubscriptionTier,
  SUBSCRIPTION_TIERS,
  SubscriptionTierInfo,
} from "@app/shared/models/economy.model";

@Component({
  selector: "app-subscription",
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="sub-page">
      <h1>Subscription</h1>

      @if (loading()) {
        <div class="sub-page__loading">Loading subscription info...</div>
      } @else if (error()) {
        <div class="sub-page__error">
          <p>{{ error() }}</p>
          <button class="btn btn--primary" (click)="reload()">Retry</button>
        </div>
      } @else {
        <!-- Current Plan -->
        @if (subscription(); as sub) {
          <div class="current-plan">
            <h2>Current Plan</h2>
            <div class="current-plan__card">
              <span class="current-plan__tier">{{ tierLabel(sub.tier) }}</span>
              <div class="current-plan__status">
                @if (sub.isActive) {
                  <span class="badge badge--active">Active</span>
                } @else {
                  <span class="badge badge--expired">Expired</span>
                }
              </div>
              <div class="current-plan__dates">
                <span>Started: {{ sub.startedAt | date: "mediumDate" }}</span>
                <span>Expires: {{ sub.expiresAt | date: "mediumDate" }}</span>
              </div>
            </div>

            <!-- Trial Info -->
            @if (trialDaysRemaining() !== null) {
              <div
                class="trial-banner"
                [class.trial-banner--urgent]="trialDaysRemaining()! <= 7"
              >
                @if (trialDaysRemaining()! > 0) {
                  <p>
                    🕐 {{ trialDaysRemaining() }} days remaining in your free
                    trial.
                    @if (trialDaysRemaining()! <= 7) {
                      Upgrade now to keep your hub active!
                    }
                  </p>
                } @else {
                  <p>⚠️ Your trial has expired. Upgrade to reactivate.</p>
                }
              </div>
            }
          </div>
        } @else {
          <div class="no-plan">
            <p>No active subscription for this hub.</p>
            @if (!startingTrial()) {
              <button class="btn btn--primary" (click)="startTrial()">
                Start Free Trial
              </button>
            } @else {
              <button class="btn btn--primary" disabled>
                Starting trial...
              </button>
            }
          </div>
        }

        <!-- Tier Comparison -->
        <h2>Compare Plans</h2>
        <div class="tier-table-wrapper">
          <table class="tier-table">
            <thead>
              <tr>
                <th>Feature</th>
                @for (t of tiers; track t.tier) {
                  <th
                    [class.tier-table__current]="
                      subscription()?.tier === t.tier
                    "
                  >
                    {{ t.label }}
                    <div class="tier-table__price">{{ t.priceLabel }}</div>
                  </th>
                }
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Hub limit</td>
                @for (t of tiers; track t.tier) {
                  <td>{{ t.hubLimit }}</td>
                }
              </tr>
              <tr>
                <td>Lists per hub</td>
                @for (t of tiers; track t.tier) {
                  <td>
                    {{ t.listLimit === null ? "Unlimited" : t.listLimit }}
                  </td>
                }
              </tr>
              <tr>
                <td>Sub-owners</td>
                @for (t of tiers; track t.tier) {
                  <td>{{ t.subOwnerLimit }}</td>
                }
              </tr>
              @for (featureIdx of featureIndices; track featureIdx) {
                <tr>
                  <td>{{ getFeatureLabel(featureIdx) }}</td>
                  @for (t of tiers; track t.tier) {
                    <td>
                      {{ hasFeatureAt(t, featureIdx) ? "✓" : "—" }}
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Upgrade Buttons -->
        <div class="upgrade-actions">
          @for (t of upgradableTiers(); track t.tier) {
            <button class="btn btn--primary" (click)="upgrade(t.tier)">
              Upgrade to {{ t.label }}
            </button>
          }
        </div>
      }
    </section>
  `,
  styles: [
    `
      .sub-page {
        max-width: 900px;
        margin: 0 auto;
        padding: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }
      .sub-page h1,
      .sub-page h2 {
        margin: 0;
      }
      .sub-page__loading,
      .sub-page__error {
        text-align: center;
        padding: 2rem;
        color: #666;
      }
      .sub-page__error p {
        color: #c0392b;
        margin-bottom: 1rem;
      }
      .current-plan__card {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
        padding: 1.25rem;
        border-radius: 12px;
        background: linear-gradient(135deg, #1a1a2e, #16213e);
        color: #fff;
      }
      .current-plan__tier {
        font-size: 1.5rem;
        font-weight: 700;
      }
      .current-plan__dates {
        display: flex;
        gap: 1.5rem;
        font-size: 0.85rem;
        color: #aaa;
      }
      .badge {
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
        font-size: 0.75rem;
        font-weight: 600;
      }
      .badge--active {
        background: rgba(39, 174, 96, 0.2);
        color: #27ae60;
      }
      .badge--expired {
        background: rgba(192, 57, 43, 0.2);
        color: #c0392b;
      }
      .trial-banner {
        padding: 0.75rem 1rem;
        border-radius: 8px;
        background: #fff3cd;
        color: #856404;
      }
      .trial-banner p {
        margin: 0;
      }
      .trial-banner--urgent {
        background: #f8d7da;
        color: #721c24;
      }
      .no-plan {
        text-align: center;
        padding: 2rem;
        background: #f8f9fa;
        border-radius: 12px;
      }
      .no-plan p {
        margin: 0 0 1rem;
        color: #666;
      }
      .tier-table-wrapper {
        overflow-x: auto;
      }
      .tier-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.9rem;
      }
      .tier-table th,
      .tier-table td {
        padding: 0.6rem 0.75rem;
        text-align: center;
        border-bottom: 1px solid #eee;
      }
      .tier-table th {
        background: #f8f9fa;
        font-weight: 600;
      }
      .tier-table td:first-child,
      .tier-table th:first-child {
        text-align: left;
      }
      .tier-table__current {
        background: #e8f5e9 !important;
        border-bottom: 2px solid #1db954;
      }
      .tier-table__price {
        font-size: 0.75rem;
        font-weight: 400;
        color: #666;
      }
      .upgrade-actions {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        justify-content: center;
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
    `,
  ],
})
export class SubscriptionComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly economyService = inject(EconomyService);
  private readonly toast = inject(ToastService);

  protected readonly tiers = SUBSCRIPTION_TIERS;
  protected readonly featureIndices = [3, 4, 5, 6, 7, 8];

  protected readonly subscription = signal<Subscription | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly startingTrial = signal(false);

  protected readonly trialDaysRemaining = computed<number | null>(() => {
    const sub = this.subscription();
    if (!sub?.trialEndsAt) return null;
    const diff = new Date(sub.trialEndsAt).getTime() - Date.now();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  });

  protected readonly upgradableTiers = computed(() => {
    const current = this.subscription()?.tier;
    const tierOrder: SubscriptionTier[] = ["free_trial", "monthly", "annual"];
    const currentIndex = current ? tierOrder.indexOf(current) : -1;
    return SUBSCRIPTION_TIERS.filter((t) => {
      const idx = tierOrder.indexOf(t.tier);
      return idx > currentIndex && t.tier !== "event";
    });
  });

  private hubId = "";

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    void this.loadSubscription();
  }

  protected async startTrial(): Promise<void> {
    this.startingTrial.set(true);
    try {
      const sub = await firstValueFrom(
        this.economyService.startTrial(this.hubId),
      );
      this.subscription.set(sub);
      this.toast.show("success", "Free trial started!");
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to start trial";
      this.toast.show("error", msg);
    } finally {
      this.startingTrial.set(false);
    }
  }

  protected upgrade(tier: SubscriptionTier): void {
    // Redirect to Stripe checkout for subscription upgrade
    this.toast.show("info", `Redirecting to ${tier} checkout...`);
    // Future: call economy service to create subscription checkout session
  }

  protected reload(): void {
    void this.loadSubscription();
  }

  protected tierLabel(tier: SubscriptionTier): string {
    return SUBSCRIPTION_TIERS.find((t) => t.tier === tier)?.label ?? tier;
  }

  protected getFeatureLabel(index: number): string {
    const labels = [
      "",
      "",
      "",
      "Music sources",
      "Analytics",
      "Ads",
      "Custom QR branding",
      "API access",
      "Priority support",
    ];
    return labels[index] ?? "";
  }

  protected hasFeatureAt(tier: SubscriptionTierInfo, index: number): boolean {
    return index < tier.features.length;
  }

  private async loadSubscription(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const sub = await firstValueFrom(
        this.economyService.getSubscription(this.hubId),
      );
      this.subscription.set(sub);
    } catch (err) {
      const status = (err as { status?: number }).status;
      if (status === 404) {
        this.subscription.set(null);
      } else {
        const msg =
          err instanceof Error ? err.message : "Failed to load subscription";
        this.error.set(msg);
      }
    } finally {
      this.loading.set(false);
    }
  }
}
