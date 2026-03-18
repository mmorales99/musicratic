import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  OnDestroy,
  computed,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { QueueMachineService } from "../machines/queue-machine.service";
import { NowPlayingComponent } from "../now-playing/now-playing.component";
import { VoteButtonsComponent } from "@app/features/voting/vote-buttons/vote-buttons.component";
import { VoteTallyComponent } from "@app/features/voting/vote-tally/vote-tally.component";
import { SkipNotificationComponent } from "@app/features/voting/skip-notification/skip-notification.component";
import { RequireRoleDirective } from "@app/shared/directives/require-role.directive";
import { UserRole } from "@app/shared/models/user-role.model";
import { QueueEntry } from "@app/shared/models/track.model";

@Component({
  selector: "app-queue",
  standalone: true,
  imports: [
    NowPlayingComponent,
    VoteButtonsComponent,
    VoteTallyComponent,
    SkipNotificationComponent,
    RequireRoleDirective,
  ],
  providers: [QueueMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="queue-page">
      @if (machine.isConnecting()) {
        <div class="queue-page__loading">Connecting to hub queue...</div>
      } @else if (machine.error()) {
        <div class="queue-page__error">
          <p>{{ machine.error() }}</p>
          <button class="btn btn--primary" (click)="machine.retry()">
            Retry
          </button>
        </div>
      } @else if (machine.isConnected()) {
        <!-- Now Playing -->
        <app-now-playing [nowPlaying]="machine.nowPlaying()" [hubId]="hubId" />

        <!-- Voting Controls -->
        @if (nowPlayingEntryId(); as entryId) {
          <div *appRequireRole="Visitor">
            <app-vote-buttons [hubId]="hubId" [entryId]="entryId" />
            <app-vote-tally [hubId]="hubId" [entryId]="entryId" />
          </div>
        }

        <!-- Skip Notifications -->
        <app-skip-notification [hubId]="hubId" />

        <!-- Queue List -->
        <div class="queue-page__list">
          <h2>Up Next</h2>
          @if (upcomingQueue().length === 0) {
            <div class="queue-page__empty">
              <p>The queue is empty. Search for tracks to add!</p>
            </div>
          } @else {
            <ul class="queue-list">
              @for (entry of upcomingQueue(); track entry.id) {
                <li
                  class="queue-list__item"
                  [class.queue-list__item--pending]="entry.status === 'pending'"
                >
                  <span class="queue-list__position">{{ entry.position }}</span>
                  <div class="queue-list__cover">
                    @if (entry.track.coverUrl) {
                      <img
                        [src]="entry.track.coverUrl"
                        [alt]="entry.track.title"
                        class="queue-list__cover-img"
                      />
                    } @else {
                      <div class="queue-list__cover-placeholder">♪</div>
                    }
                  </div>
                  <div class="queue-list__info">
                    <span class="queue-list__title">
                      {{ entry.track.title }}
                    </span>
                    <span class="queue-list__artist">
                      {{ entry.track.artist }}
                    </span>
                  </div>
                  <span class="queue-list__duration">
                    {{ formatDuration(entry.track.durationSeconds) }}
                  </span>
                  <span class="queue-list__source">
                    {{ entry.source === "proposal" ? "Proposed" : "List" }}
                  </span>
                  <span
                    class="queue-list__status"
                    [class.queue-list__status--pending]="
                      entry.status === 'pending'
                    "
                  >
                    {{ formatStatus(entry.status) }}
                  </span>
                </li>
              }
            </ul>
          }
        </div>
      } @else {
        <div class="queue-page__disconnected">
          <p>Not connected to a hub.</p>
        </div>
      }
    </section>
  `,
  styles: [
    `
      .queue-page {
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }
      .queue-page__loading,
      .queue-page__error,
      .queue-page__empty,
      .queue-page__disconnected {
        text-align: center;
        padding: 2rem;
        color: #666;
      }
      .queue-page__error p {
        margin-bottom: 1rem;
        color: #c0392b;
      }
      .queue-list {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .queue-list__item {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        border-radius: 8px;
        background: #f8f9fa;
        transition:
          background 0.2s,
          transform 0.2s;
      }
      .queue-list__item:hover {
        background: #e9ecef;
      }
      .queue-list__item--pending {
        opacity: 0.7;
        border-left: 3px solid #f39c12;
      }
      .queue-list__position {
        font-weight: 600;
        min-width: 1.5rem;
        text-align: center;
        color: #888;
      }
      .queue-list__cover {
        width: 48px;
        height: 48px;
        flex-shrink: 0;
      }
      .queue-list__cover-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 4px;
      }
      .queue-list__cover-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #ddd;
        border-radius: 4px;
        font-size: 1.25rem;
      }
      .queue-list__info {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .queue-list__title {
        font-weight: 500;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .queue-list__artist {
        font-size: 0.85rem;
        color: #666;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .queue-list__duration {
        font-size: 0.85rem;
        color: #888;
        white-space: nowrap;
      }
      .queue-list__source {
        font-size: 0.75rem;
        padding: 0.15rem 0.5rem;
        border-radius: 4px;
        background: #e2e8f0;
      }
      .queue-list__status {
        font-size: 0.75rem;
        padding: 0.15rem 0.5rem;
        border-radius: 4px;
        background: #d4edda;
      }
      .queue-list__status--pending {
        background: #fff3cd;
      }
    `,
  ],
})
export class QueueComponent implements OnInit, OnDestroy {
  protected readonly machine = inject(QueueMachineService);
  private readonly route = inject(ActivatedRoute);
  protected hubId = "";

  protected readonly Visitor = UserRole.Visitor;

  protected readonly upcomingQueue = computed<QueueEntry[]>(() =>
    this.machine
      .queue()
      .filter((e) => e.status !== "playing" && e.status !== "played"),
  );

  protected readonly nowPlayingEntryId = computed<string | null>(
    () => this.machine.nowPlaying()?.entry?.id ?? null,
  );

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    if (this.hubId) {
      this.machine.connect(this.hubId);
    }
  }

  ngOnDestroy(): void {
    this.machine.disconnect();
  }

  protected formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
  }

  protected formatStatus(status: string): string {
    const labels: Record<string, string> = {
      pending: "Pending Vote",
      playing: "Playing",
      played: "Played",
      skipped: "Skipped",
      removed: "Removed",
    };
    return labels[status] ?? status;
  }
}
