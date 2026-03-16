import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
  computed,
  OnDestroy,
  effect,
} from "@angular/core";
import { PlaybackService } from "@app/shared/services/playback.service";
import { AuthService } from "@app/shared/services/auth.service";
import { NowPlaying } from "@app/shared/models/playback.model";

@Component({
  selector: "app-now-playing",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (nowPlaying()?.entry; as entry) {
      <div class="now-playing">
        <!-- Album Art -->
        <div class="now-playing__art">
          @if (entry.track.coverUrl) {
            <img
              [src]="entry.track.coverUrl"
              [alt]="entry.track.title + ' cover'"
              class="now-playing__art-img"
            />
          } @else {
            <div class="now-playing__art-placeholder">♪</div>
          }
        </div>

        <!-- Track Info -->
        <div class="now-playing__info">
          <h2 class="now-playing__title">{{ entry.track.title }}</h2>
          <p class="now-playing__artist">{{ entry.track.artist }}</p>
          @if (entry.track.album) {
            <p class="now-playing__album">{{ entry.track.album }}</p>
          }
          @if (entry.source === "proposal") {
            <span class="now-playing__proposed">Proposed track</span>
          }
        </div>

        <!-- Progress -->
        <div class="now-playing__progress">
          <div class="now-playing__progress-bar">
            <div
              class="now-playing__progress-fill"
              [style.width.%]="progressPercent()"
            ></div>
          </div>
          <div class="now-playing__progress-times">
            <span>{{ formatTime(elapsed()) }}</span>
            <span>{{ formatTime(entry.track.durationSeconds) }}</span>
          </div>
        </div>

        <!-- Controls -->
        @if (isAuthenticated()) {
          <div class="now-playing__controls">
            <button
              class="btn btn--secondary btn--small"
              (click)="skip()"
              [disabled]="skipping()"
            >
              {{ skipping() ? "Skipping..." : "Skip" }}
            </button>
          </div>
        }
      </div>
    } @else {
      <div class="now-playing now-playing--empty">
        <p>No track playing</p>
      </div>
    }
  `,
  styles: [
    `
      .now-playing {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 1rem;
        padding: 1.5rem;
        border-radius: 12px;
        background: linear-gradient(135deg, #1a1a2e, #16213e);
        color: #fff;
      }
      .now-playing--empty {
        padding: 2rem;
        text-align: center;
        color: #888;
        background: #f8f9fa;
      }
      .now-playing__art {
        width: 200px;
        height: 200px;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
      }
      .now-playing__art-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }
      .now-playing__art-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #333;
        font-size: 3rem;
      }
      .now-playing__info {
        text-align: center;
      }
      .now-playing__title {
        font-size: 1.25rem;
        font-weight: 600;
        margin: 0 0 0.25rem;
      }
      .now-playing__artist {
        font-size: 1rem;
        color: #aaa;
        margin: 0;
      }
      .now-playing__album {
        font-size: 0.85rem;
        color: #777;
        margin: 0.25rem 0 0;
      }
      .now-playing__proposed {
        display: inline-block;
        margin-top: 0.5rem;
        font-size: 0.75rem;
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
        background: rgba(243, 156, 18, 0.3);
        color: #f39c12;
      }
      .now-playing__progress {
        width: 100%;
        max-width: 400px;
      }
      .now-playing__progress-bar {
        width: 100%;
        height: 4px;
        background: rgba(255, 255, 255, 0.2);
        border-radius: 2px;
        overflow: hidden;
      }
      .now-playing__progress-fill {
        height: 100%;
        background: #1db954;
        border-radius: 2px;
        transition: width 1s linear;
      }
      .now-playing__progress-times {
        display: flex;
        justify-content: space-between;
        font-size: 0.75rem;
        color: #aaa;
        margin-top: 0.25rem;
      }
      .now-playing__controls {
        margin-top: 0.5rem;
      }
    `,
  ],
})
export class NowPlayingComponent implements OnDestroy {
  private readonly playbackService = inject(PlaybackService);
  private readonly authService = inject(AuthService);

  readonly nowPlaying = input<NowPlaying | null>(null);
  readonly hubId = input<string>("");

  protected readonly skipping = signal(false);
  protected readonly elapsed = signal(0);

  protected readonly isAuthenticated = this.authService.authenticated;

  protected readonly progressPercent = computed<number>(() => {
    const np = this.nowPlaying();
    if (!np?.entry) return 0;
    const duration = np.entry.track.durationSeconds;
    if (duration <= 0) return 0;
    return Math.min((this.elapsed() / duration) * 100, 100);
  });

  private timerId: ReturnType<typeof setInterval> | null = null;

  constructor() {
    effect(() => {
      const np = this.nowPlaying();
      this.stopTimer();
      if (np?.entry && np.isPlaying && np.startedAt) {
        const startMs = new Date(np.startedAt).getTime();
        const nowMs = Date.now();
        const initialElapsed = Math.floor((nowMs - startMs) / 1000);
        this.elapsed.set(Math.max(initialElapsed, 0));
        this.startTimer(np.entry.track.durationSeconds);
      } else {
        this.elapsed.set(0);
      }
    });
  }

  protected async skip(): Promise<void> {
    const id = this.hubId();
    if (!id) return;
    this.skipping.set(true);
    try {
      await new Promise<void>((resolve, reject) => {
        this.playbackService.skipTrack(id).subscribe({
          next: () => resolve(),
          error: (err) => reject(err),
        });
      });
    } finally {
      this.skipping.set(false);
    }
  }

  protected formatTime(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s.toString().padStart(2, "0")}`;
  }

  private startTimer(durationSeconds: number): void {
    this.timerId = setInterval(() => {
      const current = this.elapsed();
      if (current >= durationSeconds) {
        this.stopTimer();
        return;
      }
      this.elapsed.set(current + 1);
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }
}
