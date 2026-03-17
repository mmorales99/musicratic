import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  OnInit,
  DestroyRef,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ReportsService } from "../../services/reports.service";
import { Report, ReportDetail, ReportType } from "../../models/report.model";

@Component({
  selector: "app-reports-screen",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="reports">
      <div class="reports__header">
        <h1>Reports</h1>
        <div class="reports__filters">
          <button
            class="reports__filter-btn"
            [class.reports__filter-btn--active]="activeType() === 'weekly'"
            (click)="loadReports('weekly')"
          >
            Weekly
          </button>
          <button
            class="reports__filter-btn"
            [class.reports__filter-btn--active]="activeType() === 'monthly'"
            (click)="loadReports('monthly')"
          >
            Monthly
          </button>
        </div>
      </div>

      @if (loading()) {
        <div class="reports__loading">Loading reports...</div>
      } @else if (error()) {
        <div class="reports__error">
          <p>{{ error() }}</p>
          <button class="btn btn--primary" (click)="loadReports(activeType())">
            Retry
          </button>
        </div>
      } @else if (reports().length === 0) {
        <div class="reports__empty">
          <p>No {{ activeType() }} reports available yet.</p>
        </div>
      } @else {
        <div class="reports__list">
          @for (report of reports(); track report.id) {
            <div
              class="report-card"
              [class.report-card--expanded]="expandedId() === report.id"
            >
              <button
                class="report-card__header"
                (click)="toggleReport(report.id)"
              >
                <div class="report-card__title-row">
                  <h3 class="report-card__title">{{ report.title }}</h3>
                  <span class="report-card__badge">{{ report.type }}</span>
                </div>
                <p class="report-card__period">
                  {{ formatDate(report.periodStart) }} —
                  {{ formatDate(report.periodEnd) }}
                </p>
                <p class="report-card__summary">{{ report.summary }}</p>
                <span class="report-card__chevron">
                  {{ expandedId() === report.id ? "▲" : "▼" }}
                </span>
              </button>

              @if (expandedId() === report.id) {
                @if (detailLoading()) {
                  <div class="report-card__detail-loading">
                    Loading details...
                  </div>
                } @else if (detail(); as d) {
                  <div class="report-card__detail">
                    <!-- Top Tracks -->
                    @if (d.topTracks.length > 0) {
                      <div class="report-card__section">
                        <h4>Top Tracks</h4>
                        <ul class="report-card__tracks">
                          @for (track of d.topTracks; track track.trackId) {
                            <li class="report-card__track">
                              @if (track.coverUrl) {
                                <img
                                  [src]="track.coverUrl"
                                  [alt]="track.title"
                                  class="report-card__cover"
                                />
                              } @else {
                                <span class="report-card__cover-placeholder">
                                  ♪
                                </span>
                              }
                              <span class="report-card__track-title">
                                {{ track.title }}
                              </span>
                              <span class="report-card__track-artist">
                                {{ track.artist }}
                              </span>
                              <span class="report-card__track-plays">
                                {{ track.plays }} plays
                              </span>
                              <span class="report-card__track-upvote">
                                {{ formatPercent(track.upvotePercent) }} ↑
                              </span>
                            </li>
                          }
                        </ul>
                      </div>
                    }

                    <!-- Most Active Voters -->
                    @if (d.mostActiveVoters.length > 0) {
                      <div class="report-card__section">
                        <h4>Most Active Voters</h4>
                        <ul class="report-card__voters">
                          @for (
                            voter of d.mostActiveVoters;
                            track voter.userId
                          ) {
                            <li class="report-card__voter">
                              @if (voter.avatarUrl) {
                                <img
                                  [src]="voter.avatarUrl"
                                  [alt]="voter.displayName"
                                  class="report-card__avatar"
                                />
                              } @else {
                                <span class="report-card__avatar-placeholder">
                                  👤
                                </span>
                              }
                              <span>{{ voter.displayName }}</span>
                              <span class="report-card__voter-count">
                                {{ voter.totalVotes }} votes
                              </span>
                            </li>
                          }
                        </ul>
                      </div>
                    }

                    <!-- Revenue Summary -->
                    <div class="report-card__section">
                      <h4>Revenue Summary</h4>
                      <dl class="report-card__revenue">
                        <dt>Total Revenue</dt>
                        <dd>
                          {{ d.revenueSummary.currency }}
                          {{ d.revenueSummary.totalRevenue.toFixed(2) }}
                        </dd>
                        <dt>Coins Purchased</dt>
                        <dd>{{ d.revenueSummary.coinsPurchased }}</dd>
                        <dt>Coins Spent</dt>
                        <dd>{{ d.revenueSummary.coinsSpent }}</dd>
                        <dt>Subscription Revenue</dt>
                        <dd>
                          {{ d.revenueSummary.currency }}
                          {{ d.revenueSummary.subscriptionRevenue.toFixed(2) }}
                        </dd>
                      </dl>
                    </div>

                    <!-- Suggestions -->
                    @if (d.suggestions.length > 0) {
                      <div class="report-card__section">
                        <h4>Actionable Suggestions</h4>
                        <ul class="report-card__suggestions">
                          @for (suggestion of d.suggestions; track $index) {
                            <li>{{ suggestion }}</li>
                          }
                        </ul>
                      </div>
                    }
                  </div>
                }
              }
            </div>
          }
        </div>
      }
    </section>
  `,
  styles: `
    .reports {
      padding: 1.5rem;
      max-width: 900px;
      margin: 0 auto;
    }
    .reports__header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .reports__filters {
      display: flex;
      gap: 0.5rem;
    }
    .reports__filter-btn {
      padding: 0.5rem 1rem;
      border: 1px solid #ddd;
      background: #fff;
      border-radius: 4px;
      cursor: pointer;
    }
    .reports__filter-btn--active {
      background: #6366f1;
      color: #fff;
      border-color: #6366f1;
    }
    .reports__loading,
    .reports__error,
    .reports__empty {
      text-align: center;
      padding: 3rem 1rem;
      color: #666;
    }
    .reports__list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .report-card {
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      overflow: hidden;
      background: #fff;
    }
    .report-card--expanded {
      border-color: #6366f1;
    }
    .report-card__header {
      display: block;
      width: 100%;
      text-align: left;
      padding: 1rem 1.25rem;
      border: none;
      background: none;
      cursor: pointer;
      position: relative;
    }
    .report-card__title-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .report-card__title {
      margin: 0;
      font-size: 1.1rem;
    }
    .report-card__badge {
      font-size: 0.75rem;
      padding: 0.15rem 0.5rem;
      border-radius: 999px;
      background: #e0e7ff;
      color: #4338ca;
      text-transform: capitalize;
    }
    .report-card__period {
      margin: 0.3rem 0 0;
      font-size: 0.85rem;
      color: #888;
    }
    .report-card__summary {
      margin: 0.4rem 0 0;
      font-size: 0.9rem;
      color: #555;
    }
    .report-card__chevron {
      position: absolute;
      top: 1rem;
      right: 1.25rem;
      font-size: 0.8rem;
    }

    .report-card__detail-loading {
      padding: 1rem 1.25rem;
      text-align: center;
      color: #888;
    }
    .report-card__detail {
      padding: 0 1.25rem 1.25rem;
      border-top: 1px solid #e5e7eb;
    }
    .report-card__section {
      margin-top: 1rem;
    }
    .report-card__section h4 {
      margin: 0 0 0.5rem;
      font-size: 0.95rem;
    }

    .report-card__tracks,
    .report-card__voters,
    .report-card__suggestions {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .report-card__track,
    .report-card__voter {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      font-size: 0.9rem;
    }
    .report-card__cover,
    .report-card__avatar {
      width: 32px;
      height: 32px;
      border-radius: 4px;
      object-fit: cover;
    }
    .report-card__cover-placeholder,
    .report-card__avatar-placeholder {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f3f4f6;
      border-radius: 4px;
    }
    .report-card__track-plays,
    .report-card__track-upvote,
    .report-card__voter-count {
      margin-left: auto;
      color: #888;
      font-size: 0.85rem;
    }

    .report-card__revenue {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 0.25rem 1rem;
      margin: 0;
      font-size: 0.9rem;
    }
    .report-card__revenue dt {
      color: #666;
    }
    .report-card__revenue dd {
      margin: 0;
      font-weight: 500;
    }

    .report-card__suggestions li {
      font-size: 0.9rem;
      padding: 0.4rem 0.6rem;
      background: #fef3c7;
      border-radius: 4px;
    }
  `,
})
export class ReportsScreenComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly reportsService = inject(ReportsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly reports = signal<Report[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly activeType = signal<ReportType>("weekly");

  readonly expandedId = signal<string | null>(null);
  readonly detail = signal<ReportDetail | null>(null);
  readonly detailLoading = signal(false);

  private hubId = "";

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    this.loadReports("weekly");
  }

  loadReports(type: ReportType): void {
    this.activeType.set(type);
    this.loading.set(true);
    this.error.set(null);
    this.expandedId.set(null);
    this.detail.set(null);

    this.reportsService
      .getReports(this.hubId, type)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (reports) => {
          this.reports.set(reports);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err?.error?.detail ?? "Failed to load reports.");
          this.loading.set(false);
        },
      });
  }

  toggleReport(reportId: string): void {
    if (this.expandedId() === reportId) {
      this.expandedId.set(null);
      this.detail.set(null);
      return;
    }

    this.expandedId.set(reportId);
    this.detailLoading.set(true);
    this.detail.set(null);

    this.reportsService
      .getReportDetail(reportId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (d) => {
          this.detail.set(d);
          this.detailLoading.set(false);
        },
        error: () => {
          this.detailLoading.set(false);
        },
      });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  }

  formatPercent(value: number): string {
    return `${value.toFixed(1)}%`;
  }
}
