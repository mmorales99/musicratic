import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  computed,
  signal,
} from "@angular/core";

@Component({
  selector: "app-star-rating",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="star-rating"
      [class.star-rating--interactive]="interactive()"
      role="group"
      [attr.aria-label]="'Rating: ' + rating() + ' out of 5'"
    >
      @for (star of stars(); track star.index) {
        <button
          type="button"
          class="star-rating__star"
          [class.star-rating__star--filled]="star.filled"
          [class.star-rating__star--half]="star.half"
          [disabled]="!interactive()"
          [attr.aria-label]="'Rate ' + (star.index + 1) + ' stars'"
          (click)="onStarClick(star.index + 1)"
          (mouseenter)="onHover(star.index + 1)"
          (mouseleave)="onHoverEnd()"
        >
          ★
        </button>
      }
      @if (showValue()) {
        <span class="star-rating__value">{{ displayValue() }}</span>
      }
    </div>
  `,
  styles: `
    .star-rating {
      display: inline-flex;
      align-items: center;
      gap: 2px;
    }
    .star-rating__star {
      background: none;
      border: none;
      font-size: 1.5rem;
      color: #d1d5db;
      cursor: default;
      padding: 0;
      line-height: 1;
      transition: color 0.15s ease;
    }
    .star-rating--interactive .star-rating__star {
      cursor: pointer;
    }
    .star-rating--interactive .star-rating__star:hover {
      transform: scale(1.1);
    }
    .star-rating__star--filled {
      color: #f59e0b;
    }
    .star-rating__star--half {
      color: #fbbf24;
    }
    .star-rating__value {
      margin-left: 0.5rem;
      font-weight: 600;
      font-size: 0.9rem;
    }
  `,
})
export class StarRatingComponent {
  readonly rating = input<number>(0);
  readonly interactive = input<boolean>(false);
  readonly showValue = input<boolean>(false);
  readonly ratingChange = output<number>();

  private readonly hoverRating = signal(0);

  readonly displayValue = computed((): string => {
    return this.rating().toFixed(1);
  });

  readonly stars = computed(() => {
    const current = this.hoverRating() || this.rating();
    return Array.from({ length: 5 }, (_, i) => ({
      index: i,
      filled: i + 1 <= Math.floor(current),
      half: i + 1 > Math.floor(current) && i < current,
    }));
  });

  onStarClick(value: number): void {
    if (!this.interactive()) return;
    this.ratingChange.emit(value);
  }

  onHover(value: number): void {
    if (!this.interactive()) return;
    this.hoverRating.set(value);
  }

  onHoverEnd(): void {
    this.hoverRating.set(0);
  }
}
