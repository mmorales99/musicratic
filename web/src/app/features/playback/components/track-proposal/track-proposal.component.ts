import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  OnDestroy,
  DestroyRef,
  signal,
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { FormControl, ReactiveFormsModule } from "@angular/forms";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { debounceTime, distinctUntilChanged } from "rxjs";
import { ToastService } from "@app/shared/services/toast.service";
import { TrackSearchResult } from "@app/shared/models/playback.model";
import { MusicProvider } from "@app/shared/models/hub.model";
import { ProposalMachineService } from "../../machines/proposal-machine.service";
import {
  ProposalConfirmDialogComponent,
} from "../proposal-confirm-dialog/proposal-confirm-dialog.component";

type ProviderTab = "all" | MusicProvider;

@Component({
  selector: "app-track-proposal",
  standalone: true,
  imports: [ReactiveFormsModule, ProposalConfirmDialogComponent],
  providers: [ProposalMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="proposal">
      <h2 class="proposal__title">Propose a Track</h2>

      <!-- Search Input -->
      <div class="proposal__search">
        <input
          type="text"
          [formControl]="searchControl"
          placeholder="Search for tracks..."
          class="proposal__input"
        />
      </div>

      <!-- Provider Tabs -->
      <div class="proposal__tabs">
        @for (tab of providerTabs; track tab.value) {
          <button
            class="proposal__tab"
            [class.proposal__tab--active]="activeTab() === tab.value"
            (click)="selectTab(tab.value)"
          >
            {{ tab.label }}
          </button>
        }
      </div>

      <!-- Loading -->
      @if (machine.isSearching()) {
        <div class="proposal__loading">Searching...</div>
      }

      <!-- Error -->
      @if (machine.isError()) {
        <div class="proposal__error">
          {{ machine.error() }}
          <button class="btn btn--small" (click)="machine.reset()">
            Try Again
          </button>
        </div>
      }

      <!-- Success -->
      @if (machine.isSuccess()) {
        <div class="proposal__success">
          <span>Track proposed successfully!</span>
          <button class="btn btn--primary btn--small" (click)="goToQueue()">
            View Queue
          </button>
          <button class="btn btn--small" (click)="machine.reset()">
            Propose Another
          </button>
        </div>
      }

      <!-- Results list -->
      @if (!machine.isSuccess() && machine.searchResults().length > 0) {
        <ul class="proposal__results">
          @for (track of machine.searchResults(); track track.id) {
            <li class="proposal__result">
              <div class="proposal__cover">
                @if (track.coverUrl) {
                  <img
                    [src]="track.coverUrl"
                    [alt]="track.title"
                    class="proposal__cover-img"
                  />
                } @else {
                  <div class="proposal__cover-placeholder">♪</div>
                }
              </div>
              <div class="proposal__info">
                <span class="proposal__track-title">{{ track.title }}</span>
                <span class="proposal__artist">{{ track.artist }}</span>
              </div>
              <span class="proposal__duration">
                {{ formatDuration(track.durationSeconds) }}
              </span>
              <button
                class="btn btn--primary btn--small"
                [disabled]="machine.isProposing() || machine.isConfirming()"
                (click)="onSelectTrack(track)"
              >
                Propose
              </button>
            </li>
          }
        </ul>
      }

      <!-- Confirm dialog -->
      @if (machine.isConfirming() && machine.selectedTrack()) {
        <app-proposal-confirm-dialog
          [track]="machine.selectedTrack()!"
          [price]="machine.trackPrice()"
          [wallet]="machine.wallet()"
          (confirm)="machine.confirm()"
          (cancel)="machine.cancel()"
        />
      }
    </div>
  `,
  styles: [
    `
      .proposal {
        display: flex;
        flex-direction: column;
        gap: 1rem;
        max-width: 640px;
        margin: 0 auto;
        padding: 1.5rem;
      }
      .proposal__title {
        font-size: 1.5rem;
        font-weight: 600;
        margin: 0;
      }
      .proposal__input {
        width: 100%;
        padding: 0.75rem 1rem;
        font-size: 1rem;
        border: 2px solid #e2e8f0;
        border-radius: 8px;
        outline: none;
        transition: border-color 0.2s;
      }
      .proposal__input:focus {
        border-color: #1db954;
      }
      .proposal__tabs {
        display: flex;
        gap: 0.5rem;
      }
      .proposal__tab {
        padding: 0.4rem 1rem;
        border: 1px solid #e2e8f0;
        border-radius: 20px;
        background: #fff;
        cursor: pointer;
        font-size: 0.85rem;
        transition: background 0.2s, color 0.2s;
      }
      .proposal__tab--active {
        background: #1db954;
        color: #fff;
        border-color: #1db954;
      }
      .proposal__loading,
      .proposal__error,
      .proposal__success {
        text-align: center;
        padding: 1.5rem;
        border-radius: 8px;
      }
      .proposal__loading { color: #666; }
      .proposal__error {
        color: #c0392b;
        background: #fef2f2;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        align-items: center;
      }
      .proposal__success {
        color: #059669;
        background: #ecfdf5;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        align-items: center;
      }
      .proposal__results {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .proposal__result {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        border-radius: 8px;
        background: #f8f9fa;
        transition: background 0.2s;
      }
      .proposal__result:hover { background: #e9ecef; }
      .proposal__cover { width: 48px; height: 48px; flex-shrink: 0; }
      .proposal__cover-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 4px;
      }
      .proposal__cover-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #ddd;
        border-radius: 4px;
        font-size: 1.25rem;
      }
      .proposal__info {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .proposal__track-title {
        font-weight: 500;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .proposal__artist { font-size: 0.85rem; color: #666; }
      .proposal__duration {
        font-size: 0.85rem;
        color: #888;
        white-space: nowrap;
      }
    `,
  ],
})
export class TrackProposalComponent implements OnInit, OnDestroy {
  protected readonly machine = inject(ProposalMachineService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly searchControl = new FormControl("", {
    nonNullable: true,
  });
  protected readonly activeTab = signal<ProviderTab>("all");

  protected readonly providerTabs: { value: ProviderTab; label: string }[] = [
    { value: "all", label: "All" },
    { value: "spotify", label: "Spotify" },
    { value: "youtube_music", label: "YouTube Music" },
  ];

  private hubId = "";

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    this.machine.setHub(this.hubId);

    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((query: string) => {
        if (query.trim().length >= 2) {
          const provider =
            this.activeTab() === "all"
              ? undefined
              : (this.activeTab() as MusicProvider);
          this.machine.search(query.trim(), provider);
        }
      });
  }

  ngOnDestroy(): void {
    // Cleanup handled by takeUntilDestroyed
  }

  protected selectTab(tab: ProviderTab): void {
    this.activeTab.set(tab);
    const query = this.searchControl.value;
    if (query.trim().length >= 2) {
      const provider = tab === "all" ? undefined : (tab as MusicProvider);
      this.machine.search(query.trim(), provider);
    }
  }

  protected onSelectTrack(track: TrackSearchResult): void {
    this.machine.selectTrack(track);
  }

  protected goToQueue(): void {
    this.toast.show("success", "Track added to the queue!");
    this.router.navigate(["/hub", this.hubId, "queue"]);
  }

  protected formatDuration(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
  }
}
