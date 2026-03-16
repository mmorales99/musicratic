import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
  OnInit,
  OnDestroy,
  DestroyRef,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { FormControl, ReactiveFormsModule } from "@angular/forms";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { debounceTime, distinctUntilChanged, switchMap, of } from "rxjs";
import { TrackService } from "@app/shared/services/track.service";
import {
  TrackSearchResult,
  ProposeTrackRequest,
} from "@app/shared/models/playback.model";
import { MusicProvider } from "@app/shared/models/hub.model";

type ProviderTab = "all" | MusicProvider;

@Component({
  selector: "app-track-search",
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="track-search">
      <!-- Search Input -->
      <div class="track-search__input-wrap">
        <input
          type="text"
          [formControl]="searchControl"
          placeholder="Search for tracks..."
          class="track-search__input"
        />
      </div>

      <!-- Provider Tabs -->
      <div class="track-search__tabs">
        @for (tab of providerTabs; track tab.value) {
          <button
            class="track-search__tab"
            [class.track-search__tab--active]="activeTab() === tab.value"
            (click)="selectTab(tab.value)"
          >
            {{ tab.label }}
          </button>
        }
      </div>

      <!-- Results -->
      @if (loading()) {
        <div class="track-search__loading">Searching...</div>
      } @else if (errorMsg()) {
        <div class="track-search__error">{{ errorMsg() }}</div>
      } @else if (hasSearched() && results().length === 0) {
        <div class="track-search__empty">
          No tracks found. Try a different search.
        </div>
      } @else if (results().length > 0) {
        <ul class="track-search__results">
          @for (track of results(); track track.id) {
            <li class="track-search__result">
              <div class="track-search__cover">
                @if (track.coverUrl) {
                  <img
                    [src]="track.coverUrl"
                    [alt]="track.title"
                    class="track-search__cover-img"
                  />
                } @else {
                  <div class="track-search__cover-placeholder">♪</div>
                }
              </div>
              <div class="track-search__info">
                <span class="track-search__title">{{ track.title }}</span>
                <span class="track-search__artist">{{ track.artist }}</span>
              </div>
              <span class="track-search__duration">
                {{ formatDuration(track.durationSeconds) }}
              </span>
              <button
                class="btn btn--primary btn--small"
                [disabled]="proposingId() === track.id"
                (click)="propose(track)"
              >
                {{
                  proposingId() === track.id ? "Adding..." : "Add to Queue"
                }}
              </button>
            </li>
          }
        </ul>
      }
    </div>
  `,
  styles: [
    `
      .track-search {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .track-search__input-wrap {
        position: relative;
      }
      .track-search__input {
        width: 100%;
        padding: 0.75rem 1rem;
        font-size: 1rem;
        border: 2px solid #e2e8f0;
        border-radius: 8px;
        outline: none;
        transition: border-color 0.2s;
      }
      .track-search__input:focus {
        border-color: #1db954;
      }
      .track-search__tabs {
        display: flex;
        gap: 0.5rem;
      }
      .track-search__tab {
        padding: 0.4rem 1rem;
        border: 1px solid #e2e8f0;
        border-radius: 20px;
        background: #fff;
        cursor: pointer;
        font-size: 0.85rem;
        transition:
          background 0.2s,
          color 0.2s;
      }
      .track-search__tab--active {
        background: #1db954;
        color: #fff;
        border-color: #1db954;
      }
      .track-search__loading,
      .track-search__error,
      .track-search__empty {
        text-align: center;
        padding: 1.5rem;
        color: #666;
      }
      .track-search__error {
        color: #c0392b;
      }
      .track-search__results {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .track-search__result {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        border-radius: 8px;
        background: #f8f9fa;
        transition: background 0.2s;
      }
      .track-search__result:hover {
        background: #e9ecef;
      }
      .track-search__cover {
        width: 48px;
        height: 48px;
        flex-shrink: 0;
      }
      .track-search__cover-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 4px;
      }
      .track-search__cover-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #ddd;
        border-radius: 4px;
        font-size: 1.25rem;
      }
      .track-search__info {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .track-search__title {
        font-weight: 500;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .track-search__artist {
        font-size: 0.85rem;
        color: #666;
      }
      .track-search__duration {
        font-size: 0.85rem;
        color: #888;
        white-space: nowrap;
      }
    `,
  ],
})
export class TrackSearchComponent implements OnInit, OnDestroy {
  private readonly trackService = inject(TrackService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);

  /** Hub ID — provided via input (dialog) or route param (standalone). */
  readonly hubId = input<string>("");
  private resolvedHubId = "";

  protected readonly searchControl = new FormControl("", { nonNullable: true });
  protected readonly activeTab = signal<ProviderTab>("all");
  protected readonly results = signal<TrackSearchResult[]>([]);
  protected readonly loading = signal(false);
  protected readonly errorMsg = signal<string | null>(null);
  protected readonly hasSearched = signal(false);
  protected readonly proposingId = signal<string | null>(null);

  protected readonly providerTabs: { value: ProviderTab; label: string }[] = [
    { value: "all", label: "All" },
    { value: "spotify", label: "Spotify" },
    { value: "youtube_music", label: "YouTube Music" },
  ];

  ngOnInit(): void {
    this.resolvedHubId =
      this.hubId() || this.route.snapshot.paramMap.get("hubId") || "";

    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          if (!query || query.trim().length < 2) {
            this.results.set([]);
            this.hasSearched.set(false);
            return of(null);
          }
          this.loading.set(true);
          this.errorMsg.set(null);
          const provider =
            this.activeTab() === "all" ? undefined : this.activeTab();
          return this.trackService.searchTracks(
            query.trim(),
            provider as MusicProvider | undefined,
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response) {
            this.results.set(response.results);
            this.hasSearched.set(true);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMsg.set(
            err instanceof Error ? err.message : "Search failed",
          );
        },
      });
  }

  ngOnDestroy(): void {
    // cleanup handled by takeUntilDestroyed
  }

  protected selectTab(tab: ProviderTab): void {
    this.activeTab.set(tab);
    // Re-trigger search with current query and new provider
    const query = this.searchControl.value;
    if (query && query.trim().length >= 2) {
      this.performSearch(query.trim(), tab);
    }
  }

  protected async propose(track: TrackSearchResult): Promise<void> {
    this.proposingId.set(track.id);
    const request: ProposeTrackRequest = {
      trackId: track.id,
      source: track.provider,
      externalId: track.externalId,
      title: track.title,
      artist: track.artist,
      album: track.album ?? undefined,
      durationSeconds: track.durationSeconds,
      coverUrl: track.coverUrl ?? undefined,
    };

    try {
      await new Promise<void>((resolve, reject) => {
        this.trackService.proposeTrack(this.resolvedHubId, request).subscribe({
          next: () => resolve(),
          error: (err) => reject(err),
        });
      });
    } finally {
      this.proposingId.set(null);
    }
  }

  protected formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
  }

  private performSearch(query: string, tab: ProviderTab): void {
    this.loading.set(true);
    this.errorMsg.set(null);
    const provider = tab === "all" ? undefined : (tab as MusicProvider);
    this.trackService
      .searchTracks(query, provider)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          this.results.set(response.results);
          this.hasSearched.set(true);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMsg.set(
            err instanceof Error ? err.message : "Search failed",
          );
        },
      });
  }
}
