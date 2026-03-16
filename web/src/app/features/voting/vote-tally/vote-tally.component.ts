import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
  computed,
  OnInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
} from "@angular/core";
import { Subscription, firstValueFrom } from "rxjs";
import { VoteService } from "@app/shared/services/vote.service";
import { VoteTally } from "@app/shared/models/vote.model";

const SKIP_THRESHOLD = 65;
const WARNING_THRESHOLD = 50;

@Component({
  selector: "app-vote-tally",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loaded()) {
      <div
        class="vote-tally"
        [class.vote-tally--warning]="isNearThreshold()"
        [class.vote-tally--danger]="isAtThreshold()"
      >
        <!-- Progress Bar -->
        <div class="vote-tally__bar">
          <div
            class="vote-tally__bar-up"
            [style.width.%]="upPercent()"
          ></div>
          <div
            class="vote-tally__bar-down"
            [style.width.%]="downPercent()"
          ></div>
        </div>

        <!-- Labels -->
        <div class="vote-tally__labels">
          <span class="vote-tally__up-label">
            👍 {{ upPercent() }}%
          </span>
          <span class="vote-tally__total">
            {{ totalVoters() }} vote{{ totalVoters() === 1 ? "" : "s" }}
          </span>
          <span class="vote-tally__down-label">
            {{ downPercent() }}% 👎
          </span>
        </div>

        <!-- Threshold Warning -->
        @if (isNearThreshold() && !isAtThreshold()) {
          <div class="vote-tally__warning">
            ⚠️ Approaching skip threshold ({{ downPercent() }}% / {{ skipThreshold }}%)
          </div>
        }
        @if (isAtThreshold()) {
          <div class="vote-tally__danger">
            🛑 Skip threshold reached ({{ downPercent() }}% downvotes)
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      .vote-tally {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        background: #f8f9fa;
        transition: background 0.3s;
      }
      .vote-tally--warning {
        background: #fff8e1;
      }
      .vote-tally--danger {
        background: #ffebee;
      }
      .vote-tally__bar {
        display: flex;
        height: 8px;
        border-radius: 4px;
        overflow: hidden;
        background: #e0e0e0;
      }
      .vote-tally__bar-up {
        background: #27ae60;
        transition: width 0.5s ease-out;
      }
      .vote-tally__bar-down {
        background: #c0392b;
        transition: width 0.5s ease-out;
      }
      .vote-tally__labels {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 0.85rem;
      }
      .vote-tally__up-label {
        color: #27ae60;
        font-weight: 600;
      }
      .vote-tally__total {
        color: #888;
        font-size: 0.8rem;
      }
      .vote-tally__down-label {
        color: #c0392b;
        font-weight: 600;
      }
      .vote-tally__warning {
        font-size: 0.75rem;
        color: #e67e22;
        text-align: center;
        animation: fadeIn 0.3s ease-out;
      }
      .vote-tally__danger {
        font-size: 0.75rem;
        color: #c0392b;
        font-weight: 600;
        text-align: center;
        animation: fadeIn 0.3s ease-out;
      }
      @keyframes fadeIn {
        from {
          opacity: 0;
        }
        to {
          opacity: 1;
        }
      }
    `,
  ],
})
export class VoteTallyComponent implements OnInit, OnDestroy, OnChanges {
  private readonly voteService = inject(VoteService);
  private wsSub: Subscription | null = null;

  readonly hubId = input.required<string>();
  readonly entryId = input.required<string>();

  protected readonly skipThreshold = SKIP_THRESHOLD;

  private readonly tally = signal<VoteTally | null>(null);
  protected readonly loaded = computed(() => this.tally() !== null);
  protected readonly totalVoters = computed(
    () => (this.tally()?.totalVoters ?? 0),
  );
  protected readonly upPercent = computed<number>(() => {
    const t = this.tally();
    if (!t || t.totalVoters === 0) return 50;
    return Math.round((t.upvotes / t.totalVoters) * 100);
  });
  protected readonly downPercent = computed<number>(() => {
    const t = this.tally();
    if (!t || t.totalVoters === 0) return 50;
    return Math.round((t.downvotes / t.totalVoters) * 100);
  });
  protected readonly isNearThreshold = computed(
    () =>
      this.downPercent() >= WARNING_THRESHOLD &&
      this.totalVoters() > 0,
  );
  protected readonly isAtThreshold = computed(
    () =>
      this.downPercent() >= SKIP_THRESHOLD &&
      this.totalVoters() > 0,
  );

  ngOnInit(): void {
    this.loadTally();
    this.subscribeWs();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes["hubId"] && !changes["hubId"].firstChange) ||
      (changes["entryId"] && !changes["entryId"].firstChange)
    ) {
      this.loadTally();
      this.subscribeWs();
    }
  }

  private async loadTally(): Promise<void> {
    const hub = this.hubId();
    const entry = this.entryId();
    if (!hub || !entry) return;

    try {
      const result = await firstValueFrom(
        this.voteService.getTally(hub, entry),
      );
      this.tally.set(result);
    } catch {
      this.tally.set({
        entryId: entry,
        upvotes: 0,
        downvotes: 0,
        totalVoters: 0,
        downvotePercentage: 0,
      });
    }
  }

  private subscribeWs(): void {
    this.wsSub?.unsubscribe();
    const entry = this.entryId();
    this.wsSub = this.voteService.onTallyUpdated().subscribe((msg) => {
      if (msg.payload.entryId === entry) {
        this.tally.set({
          entryId: msg.payload.entryId,
          upvotes: msg.payload.upvotes,
          downvotes: msg.payload.downvotes,
          totalVoters: msg.payload.totalVoters,
          downvotePercentage: msg.payload.downvotePercentage,
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.wsSub?.unsubscribe();
  }
}
