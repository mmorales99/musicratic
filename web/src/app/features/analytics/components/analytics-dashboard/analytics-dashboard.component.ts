import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
  OnInit,
  DestroyRef,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { AnalyticsService } from "../../services/analytics.service";
import {
  HubAnalytics,
  DateRange,
  TrackStat,
  VoteDistribution,
  DailyPlayCount,
} from "../../models/analytics.model";
import {
  BarChartComponent,
  BarChartItem,
} from "../bar-chart/bar-chart.component";
import {
  LineChartComponent,
  LineChartPoint,
  LineChartSeries,
} from "../line-chart/line-chart.component";
import {
  AreaChartComponent,
  AreaChartPoint,
} from "../area-chart/area-chart.component";

@Component({
  selector: "app-analytics-dashboard",
  standalone: true,
  imports: [
    FormsModule,
    BarChartComponent,
    LineChartComponent,
    AreaChartComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dashboard">
      <div class="dashboard__header">
        <h1 class="dashboard__title">Hub Analytics</h1>
        <div class="dashboard__date-range">
          <label>
            From
            <input
              type="date"
              [ngModel]="startDate()"
              (ngModelChange)="onStartDateChange($event)"
            />
          </label>
          <label>
            To
            <input
              type="date"
              [ngModel]="endDate()"
              (ngModelChange)="onEndDateChange($event)"
            />
          </label>
          <button
            class="btn btn--primary btn--small"
            (click)="loadData()"
            [disabled]="loading()"
          >
            {{ loading() ? "Loading..." : "Apply" }}
          </button>
        </div>
      </div>

      @if (error()) {
        <div class="dashboard__error">{{ error() }}</div>
      }

      <!-- Stats cards -->
      <div class="dashboard__stats">
        <div class="dashboard__card">
          <span class="dashboard__card-icon">▶</span>
          <span class="dashboard__card-value">{{ analytics()?.totalPlays ?? 0 }}</span>
          <span class="dashboard__card-label">Total Plays</span>
        </div>
        <div class="dashboard__card">
          <span class="dashboard__card-icon">✓</span>
          <span class="dashboard__card-value">{{ analytics()?.totalVotes ?? 0 }}</span>
          <span class="dashboard__card-label">Total Votes</span>
        </div>
        <div class="dashboard__card">
          <span class="dashboard__card-icon">↑</span>
          <span class="dashboard__card-value">
            {{ formatPercent(analytics()?.averageUpvotePercent ?? 0) }}
          </span>
          <span class="dashboard__card-label">Avg Upvote %</span>
        </div>
        <div class="dashboard__card">
          <span class="dashboard__card-icon">♫</span>
          <span class="dashboard__card-value">
            {{ analytics()?.activeListeners ?? 0 }}
          </span>
          <span class="dashboard__card-label">Active Listeners</span>
        </div>
      </div>

      <!-- Charts -->
      <div class="dashboard__charts">
        <div class="dashboard__chart-card">
          <app-bar-chart
            [data]="topTracksData()"
            [title]="'Top 10 Most Played Tracks'"
            [barColor]="'#1db954'"
          />
        </div>
        <div class="dashboard__chart-card">
          <app-line-chart
            [data]="voteDistData()"
            [series]="voteSeries"
            [title]="'Vote Distribution Over Time'"
          />
        </div>
        <div class="dashboard__chart-card">
          <app-area-chart
            [data]="playCountData()"
            [title]="'Play Count by Day'"
            [fillColor]="'#6366f1'"
          />
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      padding: 1.5rem;
      max-width: 960px;
      margin: 0 auto;
    }
    .dashboard__header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .dashboard__title {
      font-size: 1.5rem;
      font-weight: 600;
      margin: 0;
    }
    .dashboard__date-range {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-size: 0.9rem;
    }
    .dashboard__date-range input {
      padding: 0.4rem 0.5rem;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      font-size: 0.85rem;
    }
    .dashboard__error {
      padding: 0.75rem 1rem;
      background: #fef2f2;
      color: #dc2626;
      border-radius: 8px;
      text-align: center;
    }
    .dashboard__stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
    }
    .dashboard__card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.25rem;
      padding: 1.25rem;
      background: #f8f9fa;
      border-radius: 10px;
      text-align: center;
    }
    .dashboard__card-icon { font-size: 1.25rem; }
    .dashboard__card-value {
      font-size: 1.75rem;
      font-weight: 700;
    }
    .dashboard__card-label {
      font-size: 0.85rem;
      color: #666;
    }
    .dashboard__charts {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .dashboard__chart-card {
      padding: 1.25rem;
      background: #fff;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
    }
  `],
})
export class AnalyticsDashboardComponent implements OnInit {
  private readonly analyticsService = inject(AnalyticsService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  private hubId = "";

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly analytics = signal<HubAnalytics | null>(null);

  protected readonly startDate = signal(defaultStartDate());
  protected readonly endDate = signal(todayIso());

  protected readonly voteSeries: LineChartSeries[] = [
    { name: "Upvotes", color: "#22c55e" },
    { name: "Downvotes", color: "#ef4444" },
  ];

  protected readonly topTracksData = computed<BarChartItem[]>(() => {
    const a = this.analytics();
    if (!a) return [];
    return a.topTracks.map((t: TrackStat) => ({
      label: `${t.artist} — ${t.title}`,
      value: t.plays,
    }));
  });

  protected readonly voteDistData = computed<LineChartPoint[]>(() => {
    const a = this.analytics();
    if (!a) return [];
    return a.voteDistribution.map((v: VoteDistribution) => ({
      label: v.date,
      values: [
        { name: "Upvotes", value: v.upvotes },
        { name: "Downvotes", value: v.downvotes },
      ],
    }));
  });

  protected readonly playCountData = computed<AreaChartPoint[]>(() => {
    const a = this.analytics();
    if (!a) return [];
    return a.playCountByDay.map((p: DailyPlayCount) => ({
      label: p.date,
      value: p.count,
    }));
  });

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    this.loadData();
  }

  protected loadData(): void {
    if (!this.hubId) return;
    this.loading.set(true);
    this.error.set(null);

    const dateRange: DateRange = {
      startDate: this.startDate(),
      endDate: this.endDate(),
    };

    this.analyticsService
      .getHubAnalytics(this.hubId, dateRange)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: HubAnalytics) => {
          this.analytics.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(
            err instanceof Error ? err.message : "Failed to load analytics",
          );
          this.loading.set(false);
        },
      });
  }

  protected onStartDateChange(value: string): void {
    this.startDate.set(value);
  }

  protected onEndDateChange(value: string): void {
    this.endDate.set(value);
  }

  protected formatPercent(value: number): string {
    return `${Math.round(value)}%`;
  }
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function defaultStartDate(): string {
  const d = new Date();
  d.setDate(d.getDate() - 30);
  return d.toISOString().slice(0, 10);
}
