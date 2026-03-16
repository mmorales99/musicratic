import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  OnDestroy,
  signal,
  computed,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { DecimalPipe } from "@angular/common";
import {
  Subject,
  Subscription,
  debounceTime,
  distinctUntilChanged,
} from "rxjs";
import { HubService } from "@app/shared/services/hub.service";
import {
  HubBusinessType,
  HubVisibility,
  HubSearchResult,
  SearchHubsParams,
} from "@app/shared/models/hub.model";
import { firstValueFrom } from "rxjs";

interface FilterChip<T> {
  value: T;
  label: string;
  active: boolean;
}

@Component({
  selector: "app-hub",
  standalone: true,
  imports: [RouterLink, FormsModule, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-page">
      <div class="hub-page__header">
        <h1>Hub Discovery</h1>
        <div class="hub-page__header-actions">
          <a routerLink="/hub/join" class="btn btn--secondary">Join Hub</a>
          <a routerLink="/hub/create" class="btn btn--primary">+ Create Hub</a>
        </div>
      </div>

      <!-- Search -->
      <div class="hub-page__search">
        <input
          type="text"
          class="search-input"
          placeholder="Search hubs by name..."
          [ngModel]="searchQuery()"
          (ngModelChange)="onSearchInput($event)"
        />
      </div>

      <!-- Filter chips: Business Type -->
      <div class="hub-page__filters">
        <span class="filter-label">Type:</span>
        @for (chip of typeChips(); track chip.value) {
          <button
            class="chip"
            [class.chip--active]="chip.active"
            (click)="toggleTypeFilter(chip.value)"
          >
            {{ chip.label }}
          </button>
        }

        <span class="filter-label filter-label--spaced">Visibility:</span>
        @for (chip of visibilityChips(); track chip.value) {
          <button
            class="chip"
            [class.chip--active]="chip.active"
            (click)="toggleVisibilityFilter(chip.value)"
          >
            {{ chip.label }}
          </button>
        }

        @if (hasActiveFilters()) {
          <button class="chip chip--clear" (click)="clearFilters()">
            Clear all
          </button>
        }
      </div>

      <!-- Results -->
      @if (loading()) {
        <p class="hub-page__loading">Searching hubs...</p>
      } @else if (error(); as err) {
        <p class="hub-page__error">{{ err }}</p>
      } @else if (hubs().length === 0) {
        <div class="hub-page__empty">
          <p>No hubs found.</p>
          <p>Try a different search or create your own hub!</p>
        </div>
      } @else {
        <div class="hub-grid">
          @for (hub of hubs(); track hub.id) {
            <a [routerLink]="['/hub', hub.id]" class="hub-card">
              <div class="hub-card__top">
                <span class="hub-card__name">{{ hub.name }}</span>
                <span
                  class="hub-card__status"
                  [class.hub-card__status--active]="hub.isActive"
                  >{{ hub.isActive ? "Active" : "Inactive" }}</span
                >
              </div>
              <span class="hub-card__type">{{ hub.businessType }}</span>
              <div class="hub-card__meta">
                <span class="hub-card__members"
                  >{{ hub.memberCount }} members</span
                >
                @if (hub.rating !== null) {
                  <span class="hub-card__rating"
                    >★ {{ hub.rating | number: "1.1-1" }}</span
                  >
                }
              </div>
              <span class="hub-card__visibility">{{ hub.visibility }}</span>
            </a>
          }
        </div>

        @if (hasMore()) {
          <div class="hub-page__load-more">
            <button
              class="btn btn--secondary"
              [disabled]="loadingMore()"
              (click)="loadMore()"
            >
              {{ loadingMore() ? "Loading..." : "Load More" }}
            </button>
          </div>
        }
      }
    </section>
  `,
  styles: [
    `
      .hub-page__header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.25rem;
        flex-wrap: wrap;
        gap: 0.75rem;
      }
      .hub-page__header h1 {
        font-size: 1.75rem;
        color: #e0e0e0;
        margin: 0;
      }
      .hub-page__header-actions {
        display: flex;
        gap: 0.5rem;
      }
      .hub-page__search {
        margin: 1rem 0;
      }
      .search-input {
        width: 100%;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        border: 1px solid #2a2a4a;
        background: #16213e;
        color: #e0e0e0;
        font-size: 1rem;
        box-sizing: border-box;
      }
      .search-input:focus {
        outline: none;
        border-color: #6c5ce7;
      }
      .hub-page__filters {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        align-items: center;
        margin-bottom: 1.5rem;
      }
      .filter-label {
        color: #a0a0b0;
        font-size: 0.85rem;
        font-weight: 600;
      }
      .filter-label--spaced {
        margin-left: 0.75rem;
      }
      .chip {
        padding: 0.35rem 0.85rem;
        border-radius: 20px;
        border: 1px solid #2a2a4a;
        background: transparent;
        color: #a0a0b0;
        font-size: 0.8rem;
        cursor: pointer;
        transition: all 0.15s;
      }
      .chip:hover {
        border-color: #6c5ce7;
        color: #c0c0d0;
      }
      .chip--active {
        background: #6c5ce7;
        color: #fff;
        border-color: #6c5ce7;
      }
      .chip--clear {
        background: rgba(231, 76, 60, 0.15);
        color: #e74c3c;
        border-color: rgba(231, 76, 60, 0.3);
      }
      .hub-page__loading,
      .hub-page__error,
      .hub-page__empty {
        color: #a0a0b0;
        text-align: center;
        padding: 2rem;
      }
      .hub-page__error {
        color: #e74c3c;
      }
      .hub-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
        gap: 1rem;
      }
      .hub-card {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;
        padding: 1rem 1.25rem;
        border-radius: 10px;
        background: #16213e;
        border: 1px solid #2a2a4a;
        text-decoration: none;
        transition: border-color 0.2s;
      }
      .hub-card:hover {
        border-color: #6c5ce7;
      }
      .hub-card__top {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }
      .hub-card__name {
        font-size: 1.1rem;
        font-weight: 700;
        color: #e0e0e0;
      }
      .hub-card__type {
        color: #a0a0b0;
        font-size: 0.85rem;
        text-transform: capitalize;
      }
      .hub-card__meta {
        display: flex;
        gap: 1rem;
        align-items: center;
      }
      .hub-card__members {
        color: #a0a0b0;
        font-size: 0.85rem;
      }
      .hub-card__rating {
        color: #ffa502;
        font-size: 0.85rem;
        font-weight: 600;
      }
      .hub-card__visibility {
        color: #6c5ce7;
        font-size: 0.75rem;
        text-transform: uppercase;
        font-weight: 600;
        letter-spacing: 0.05rem;
      }
      .hub-card__status {
        font-size: 0.75rem;
        padding: 0.15rem 0.5rem;
        border-radius: 4px;
      }
      .hub-card__status--active {
        background: rgba(46, 213, 115, 0.15);
        color: #2ed573;
      }
      .hub-page__load-more {
        text-align: center;
        margin-top: 1.5rem;
      }
      .btn {
        padding: 0.5rem 1.25rem;
        border-radius: 8px;
        border: none;
        font-size: 0.9rem;
        cursor: pointer;
        font-weight: 600;
        text-decoration: none;
      }
      .btn--primary {
        background: #6c5ce7;
        color: #fff;
      }
      .btn--secondary {
        background: transparent;
        color: #a0a0b0;
        border: 1px solid #2a2a4a;
      }
      .btn--secondary:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
    `,
  ],
})
export class HubComponent implements OnInit, OnDestroy {
  private readonly hubService = inject(HubService);
  private readonly searchSubject = new Subject<string>();
  private searchSub: Subscription | null = null;

  private static readonly PAGE_SIZE = 12;

  readonly searchQuery = signal("");
  readonly hubs = signal<HubSearchResult[]>([]);
  readonly loading = signal(true);
  readonly loadingMore = signal(false);
  readonly error = signal<string | null>(null);
  readonly hasMore = signal(false);
  readonly currentPage = signal(1);

  readonly typeChips = signal<FilterChip<HubBusinessType>[]>([
    { value: "bar", label: "Bar", active: false },
    { value: "restaurant", label: "Restaurant", active: false },
    { value: "gym", label: "Gym", active: false },
    { value: "store", label: "Store", active: false },
    { value: "office", label: "Office", active: false },
    { value: "custom", label: "Custom", active: false },
  ]);

  readonly visibilityChips = signal<FilterChip<HubVisibility>[]>([
    { value: "public", label: "Public", active: false },
    { value: "private", label: "Private", active: false },
  ]);

  readonly hasActiveFilters = computed(
    () =>
      this.typeChips().some((c) => c.active) ||
      this.visibilityChips().some((c) => c.active),
  );

  private get activeType(): HubBusinessType | undefined {
    return this.typeChips().find((c) => c.active)?.value;
  }

  private get activeVisibility(): HubVisibility | undefined {
    return this.visibilityChips().find((c) => c.active)?.value;
  }

  ngOnInit(): void {
    this.searchSub = this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.search(true));

    this.search(true);
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
    this.searchSubject.next(value);
  }

  toggleTypeFilter(value: HubBusinessType): void {
    this.typeChips.update((chips) =>
      chips.map((c) => ({
        ...c,
        active: c.value === value ? !c.active : false,
      })),
    );
    this.search(true);
  }

  toggleVisibilityFilter(value: HubVisibility): void {
    this.visibilityChips.update((chips) =>
      chips.map((c) => ({
        ...c,
        active: c.value === value ? !c.active : false,
      })),
    );
    this.search(true);
  }

  clearFilters(): void {
    this.typeChips.update((chips) =>
      chips.map((c) => ({ ...c, active: false })),
    );
    this.visibilityChips.update((chips) =>
      chips.map((c) => ({ ...c, active: false })),
    );
    this.searchQuery.set("");
    this.search(true);
  }

  async loadMore(): Promise<void> {
    this.loadingMore.set(true);
    this.currentPage.update((p) => p + 1);

    try {
      const envelope = await firstValueFrom(
        this.hubService.searchHubs(this.buildParams()),
      );
      this.hubs.update((current) => [...current, ...envelope.items]);
      this.hasMore.set(envelope.hasMoreItems);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load more results";
      this.error.set(message);
    } finally {
      this.loadingMore.set(false);
    }
  }

  private async search(reset: boolean): Promise<void> {
    if (reset) {
      this.currentPage.set(1);
      this.loading.set(true);
      this.error.set(null);
    }

    try {
      const envelope = await firstValueFrom(
        this.hubService.searchHubs(this.buildParams()),
      );
      this.hubs.set(envelope.items);
      this.hasMore.set(envelope.hasMoreItems);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to search hubs";
      this.error.set(message);
    } finally {
      this.loading.set(false);
    }
  }

  private buildParams(): SearchHubsParams {
    return {
      name: this.searchQuery() || undefined,
      type: this.activeType,
      visibility: this.activeVisibility,
      page: this.currentPage(),
      pageSize: HubComponent.PAGE_SIZE,
    };
  }
}
