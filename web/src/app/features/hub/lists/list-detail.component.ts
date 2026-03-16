import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
} from "@angular/core";
import { ActivatedRoute, RouterLink } from "@angular/router";
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
} from "@angular/cdk/drag-drop";
import { ListDetailMachineService } from "../machines/list-detail-machine.service";
import {
  ListTrack,
  PlayMode,
  AddTrackRequest,
} from "@app/shared/models/hub.model";

@Component({
  selector: "app-list-detail",
  standalone: true,
  imports: [DragDropModule, RouterLink],
  providers: [ListDetailMachineService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="list-detail">
      @if (machine.isLoading()) {
        <div class="list-detail__loading">Loading list...</div>
      } @else if (machine.error(); as err) {
        <div class="list-detail__error">
          <p>{{ err }}</p>
          <button class="btn btn--primary" (click)="machine.retry()">
            Retry
          </button>
        </div>
      } @else if (machine.list(); as list) {
        <!-- Header -->
        <div class="list-detail__header">
          <a [routerLink]="['/hub', hubId]" class="list-detail__back"
            >&larr; Back to Hub</a
          >
          <h1>{{ list.name }}</h1>
          <div class="list-detail__controls">
            <div class="play-mode-toggle">
              <button
                class="play-mode-toggle__btn"
                [class.play-mode-toggle__btn--active]="
                  list.playMode === 'ordered'
                "
                (click)="onSetPlayMode('ordered')"
              >
                Ordered
              </button>
              <button
                class="play-mode-toggle__btn"
                [class.play-mode-toggle__btn--active]="
                  list.playMode === 'weighted_shuffle'
                "
                (click)="onSetPlayMode('weighted_shuffle')"
              >
                Shuffle
              </button>
            </div>
            <span class="list-detail__count"
              >{{ machine.tracks().length }} tracks</span
            >
          </div>
        </div>

        <!-- Add Track -->
        <div class="list-detail__add">
          <input
            type="text"
            class="list-detail__search"
            placeholder="Add track — paste title/URL or search..."
            [value]="searchQuery()"
            (input)="searchQuery.set(asInputValue($event))"
            (keydown.enter)="onQuickAdd()"
          />
          <button class="btn btn--primary btn--small" (click)="onQuickAdd()">
            Add
          </button>
        </div>

        <!-- Track List (drag & drop) -->
        <div
          cdkDropList
          class="track-list"
          (cdkDropListDropped)="onDrop($event)"
        >
          @for (track of machine.tracks(); track track.id; let i = $index) {
            <div class="track-item" cdkDrag>
              <div class="track-item__handle" cdkDragHandle>⠿</div>
              <span class="track-item__position">{{ i + 1 }}</span>
              @if (track.coverUrl) {
                <img
                  [src]="track.coverUrl"
                  [alt]="track.title"
                  class="track-item__cover"
                />
              } @else {
                <div class="track-item__cover track-item__cover--placeholder">
                  ♪
                </div>
              }
              <div class="track-item__info">
                <span class="track-item__title">{{ track.title }}</span>
                <span class="track-item__artist">{{ track.artist }}</span>
              </div>
              <span class="track-item__duration">{{
                formatDuration(track.durationSeconds)
              }}</span>
              <span class="track-item__provider">{{ track.provider }}</span>
              <button
                class="track-item__remove"
                (click)="onRemoveTrack(track.id, track.title)"
              >
                ✕
              </button>
            </div>
          }
        </div>

        @if (machine.tracks().length === 0) {
          <p class="list-detail__empty">
            No tracks in this list. Add some above!
          </p>
        }
      }
    </section>
  `,
  styles: [
    `
      .list-detail {
        max-width: 800px;
        margin: 0 auto;
      }
      .list-detail__loading,
      .list-detail__error {
        text-align: center;
        padding: 3rem;
        color: #a0a0b0;
      }
      .list-detail__error p {
        color: #e74c3c;
        margin-bottom: 1rem;
      }
      .list-detail__back {
        color: #6c5ce7;
        text-decoration: none;
        font-size: 0.875rem;
        display: inline-block;
        margin-bottom: 0.5rem;
      }
      .list-detail__header h1 {
        font-size: 1.5rem;
        color: #e0e0e0;
        margin: 0 0 0.75rem;
      }
      .list-detail__controls {
        display: flex;
        align-items: center;
        gap: 1rem;
        margin-bottom: 1rem;
      }
      .play-mode-toggle {
        display: flex;
        border-radius: 8px;
        overflow: hidden;
      }
      .play-mode-toggle__btn {
        padding: 0.375rem 1rem;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #a0a0b0;
        cursor: pointer;
        font-size: 0.85rem;
        font-weight: 600;
      }
      .play-mode-toggle__btn--active {
        background: #6c5ce7;
        color: #fff;
        border-color: #6c5ce7;
      }
      .list-detail__count {
        color: #a0a0b0;
        font-size: 0.875rem;
      }
      .list-detail__add {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 1.25rem;
      }
      .list-detail__search {
        flex: 1;
        padding: 0.5rem 0.75rem;
        border-radius: 8px;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #e0e0e0;
        font-size: 0.95rem;
      }
      .list-detail__search:focus {
        outline: none;
        border-color: #6c5ce7;
      }
      .track-list {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
      }
      .track-item {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.625rem 0.75rem;
        border-radius: 8px;
        background: #16213e;
        border: 1px solid #2a2a4a;
        transition: border-color 0.2s;
      }
      .track-item:hover {
        border-color: #3a3a5a;
      }
      .track-item__handle {
        cursor: grab;
        color: #505070;
        font-size: 1.1rem;
        user-select: none;
      }
      .track-item__position {
        width: 1.5rem;
        text-align: right;
        color: #505070;
        font-size: 0.85rem;
      }
      .track-item__cover {
        width: 40px;
        height: 40px;
        border-radius: 4px;
        object-fit: cover;
      }
      .track-item__cover--placeholder {
        display: flex;
        align-items: center;
        justify-content: center;
        background: #2a2a4a;
        color: #6c5ce7;
        font-size: 1.2rem;
      }
      .track-item__info {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
      }
      .track-item__title {
        color: #e0e0e0;
        font-weight: 600;
        font-size: 0.9rem;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .track-item__artist {
        color: #a0a0b0;
        font-size: 0.8rem;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .track-item__duration {
        color: #a0a0b0;
        font-size: 0.8rem;
        min-width: 3rem;
        text-align: right;
      }
      .track-item__provider {
        color: #505070;
        font-size: 0.75rem;
        min-width: 4rem;
      }
      .track-item__remove {
        background: none;
        border: none;
        color: #e74c3c;
        cursor: pointer;
        font-size: 1rem;
        padding: 0.25rem;
        opacity: 0.6;
        transition: opacity 0.2s;
      }
      .track-item__remove:hover {
        opacity: 1;
      }
      .list-detail__empty {
        text-align: center;
        color: #a0a0b0;
        padding: 2rem;
      }
      .btn {
        padding: 0.5rem 1.25rem;
        border-radius: 8px;
        border: none;
        font-size: 0.9rem;
        cursor: pointer;
        font-weight: 600;
      }
      .btn--primary {
        background: #6c5ce7;
        color: #fff;
      }
      .btn--small {
        padding: 0.375rem 0.875rem;
        font-size: 0.85rem;
      }
      .cdk-drag-preview {
        background: #1a2744;
        border: 1px solid #6c5ce7;
        border-radius: 8px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
      }
      .cdk-drag-placeholder {
        opacity: 0.3;
      }
    `,
  ],
})
export class ListDetailComponent implements OnInit {
  protected readonly machine = inject(ListDetailMachineService);
  private readonly route = inject(ActivatedRoute);

  readonly searchQuery = signal("");
  hubId = "";

  ngOnInit(): void {
    this.hubId = this.route.snapshot.paramMap.get("hubId") ?? "";
    const listId = this.route.snapshot.paramMap.get("listId") ?? "";
    if (this.hubId && listId) {
      this.machine.load(this.hubId, listId);
    }
  }

  onDrop(event: CdkDragDrop<ListTrack[]>): void {
    const tracks = [...this.machine.tracks()];
    moveItemInArray(tracks, event.previousIndex, event.currentIndex);
    const movedTrack = tracks[event.currentIndex];
    this.machine.reorderTrack(movedTrack.id, event.currentIndex);
  }

  onQuickAdd(): void {
    const query = this.searchQuery().trim();
    if (!query) return;

    const request: AddTrackRequest = {
      provider: "spotify",
      externalId: "",
      title: query,
      artist: "Unknown",
      durationSeconds: 0,
    };
    this.machine.addTrack(request);
    this.searchQuery.set("");
  }

  onRemoveTrack(trackId: string, trackTitle: string): void {
    if (confirm(`Remove "${trackTitle}" from this list?`)) {
      this.machine.removeTrack(trackId);
    }
  }

  onSetPlayMode(mode: PlayMode): void {
    this.machine.setPlayMode(mode);
  }

  formatDuration(seconds: number): string {
    if (!seconds) return "--:--";
    const min = Math.floor(seconds / 60);
    const sec = seconds % 60;
    return `${min}:${sec.toString().padStart(2, "0")}`;
  }

  asInputValue(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }
}
