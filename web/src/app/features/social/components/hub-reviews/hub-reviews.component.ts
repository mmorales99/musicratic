import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  input,
  signal,
} from "@angular/core";
import { ReactiveFormsModule, FormBuilder, Validators } from "@angular/forms";
import { ReviewService } from "../../services/review.service";
import { Review, HubRating, PaginatedReviews } from "../../models/review.model";
import { StarRatingComponent } from "../star-rating/star-rating.component";
import { AuthService } from "@app/shared/services/auth.service";
import { DatePipe } from "@angular/common";

@Component({
  selector: "app-hub-reviews",
  standalone: true,
  imports: [ReactiveFormsModule, StarRatingComponent, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hub-reviews">
      <div class="hub-reviews__header">
        <h2>Reviews</h2>
        @if (hubRating(); as rating) {
          @if (rating.isVisible) {
            <div class="hub-reviews__aggregate">
              <app-star-rating
                [rating]="rating.averageRating"
                [showValue]="true"
              />
              <span class="hub-reviews__count">
                ({{ rating.totalReviews }} reviews)
              </span>
            </div>
          } @else {
            <span class="hub-reviews__min-notice">
              Needs {{ 3 - rating.totalReviews }} more reviews to show rating
            </span>
          }
        }
      </div>

      @if (authenticated()) {
        @if (showForm()) {
          <form
            [formGroup]="reviewForm"
            (ngSubmit)="submitReview()"
            class="hub-reviews__form"
          >
            <div class="hub-reviews__form-rating">
              <span>Your rating:</span>
              <app-star-rating
                [rating]="formRating()"
                [interactive]="true"
                (ratingChange)="onRatingChange($event)"
              />
            </div>
            <label class="form-field">
              Comment (optional)
              <textarea
                formControlName="comment"
                maxlength="500"
                rows="3"
                placeholder="Share your experience..."
              ></textarea>
              <span class="form-field__hint">
                {{ reviewForm.get("comment")?.value?.length || 0 }}/500
              </span>
            </label>
            <div class="hub-reviews__form-actions">
              <button
                type="submit"
                class="btn btn--primary"
                [disabled]="submitting() || !formRating()"
              >
                {{ editingReview() ? "Update Review" : "Submit Review" }}
              </button>
              @if (editingReview()) {
                <button
                  type="button"
                  class="btn btn--secondary"
                  (click)="cancelEdit()"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  class="btn btn--danger"
                  (click)="deleteReview()"
                  [disabled]="submitting()"
                >
                  Delete
                </button>
              }
            </div>
          </form>
        } @else {
          <button class="btn btn--secondary" (click)="showForm.set(true)">
            Write a Review
          </button>
        }
      }

      <div class="hub-reviews__list">
        @for (review of reviews(); track review.id) {
          <div class="hub-reviews__item">
            <div class="hub-reviews__item-header">
              @if (review.reviewerAvatarUrl) {
                <img
                  [src]="review.reviewerAvatarUrl"
                  [alt]="review.reviewerName"
                  class="hub-reviews__avatar"
                />
              } @else {
                <div class="hub-reviews__avatar-placeholder">
                  {{ review.reviewerName.charAt(0).toUpperCase() }}
                </div>
              }
              <div>
                <strong>{{ review.reviewerName }}</strong>
                <app-star-rating [rating]="review.rating" />
              </div>
              <span class="hub-reviews__date">
                {{ review.createdAt | date }}
              </span>
              @if (isOwnReview(review)) {
                <button
                  class="btn btn--small"
                  (click)="startEditReview(review)"
                >
                  Edit
                </button>
              }
            </div>
            @if (review.comment) {
              <p class="hub-reviews__comment">{{ review.comment }}</p>
            }
            @if (review.ownerResponse) {
              <div class="hub-reviews__owner-response">
                <strong>Owner response:</strong>
                <p>{{ review.ownerResponse }}</p>
              </div>
            }
          </div>
        } @empty {
          <p class="hub-reviews__empty">No reviews yet.</p>
        }
      </div>

      @if (hasMore()) {
        <button
          class="btn btn--secondary hub-reviews__load-more"
          (click)="loadMore()"
          [disabled]="loadingMore()"
        >
          {{ loadingMore() ? "Loading..." : "Load More" }}
        </button>
      }
    </section>
  `,
})
export class HubReviewsComponent implements OnInit {
  readonly hubId = input.required<string>();

  private readonly reviewService = inject(ReviewService);
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly reviews = signal<Review[]>([]);
  readonly hubRating = signal<HubRating | null>(null);
  readonly loading = signal(true);
  readonly loadingMore = signal(false);
  readonly submitting = signal(false);
  readonly showForm = signal(false);
  readonly editingReview = signal<Review | null>(null);
  readonly formRating = signal(0);
  readonly hasMore = signal(false);
  readonly authenticated = signal(false);

  private currentPage = 1;

  readonly reviewForm = this.fb.nonNullable.group({
    comment: ["", Validators.maxLength(500)],
  });

  ngOnInit(): void {
    this.authenticated.set(this.authService.authenticated());
    this.loadReviews();
    this.loadRating();
  }

  onRatingChange(rating: number): void {
    this.formRating.set(rating);
  }

  submitReview(): void {
    const rating = this.formRating();
    if (!rating) return;
    this.submitting.set(true);
    const comment = this.reviewForm.getRawValue().comment || undefined;
    const editing = this.editingReview();

    const obs$ = editing
      ? this.reviewService.updateReview(editing.id, rating, comment)
      : this.reviewService.createReview(this.hubId(), rating, comment);

    obs$.subscribe({
      next: (review: Review) => {
        if (editing) {
          this.reviews.update((list: Review[]) =>
            list.map((r: Review) => (r.id === review.id ? review : r)),
          );
        } else {
          this.reviews.update((list: Review[]) => [review, ...list]);
        }
        this.resetForm();
        this.loadRating();
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false),
    });
  }

  deleteReview(): void {
    const review = this.editingReview();
    if (!review) return;
    this.submitting.set(true);
    this.reviewService.deleteReview(review.id).subscribe({
      next: () => {
        this.reviews.update((list: Review[]) =>
          list.filter((r: Review) => r.id !== review.id),
        );
        this.resetForm();
        this.loadRating();
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false),
    });
  }

  startEditReview(review: Review): void {
    this.editingReview.set(review);
    this.formRating.set(review.rating);
    this.reviewForm.patchValue({ comment: review.comment ?? "" });
    this.showForm.set(true);
  }

  cancelEdit(): void {
    this.resetForm();
  }

  isOwnReview(review: Review): boolean {
    const user = this.authService.currentUser();
    return !!user && review.userId === user.id;
  }

  loadMore(): void {
    this.currentPage++;
    this.loadingMore.set(true);
    this.reviewService.getReviews(this.hubId(), this.currentPage).subscribe({
      next: (res: PaginatedReviews) => {
        this.reviews.update((list: Review[]) => [...list, ...res.items]);
        this.hasMore.set(res.hasMoreItems);
        this.loadingMore.set(false);
      },
      error: () => this.loadingMore.set(false),
    });
  }

  private loadReviews(): void {
    this.currentPage = 1;
    this.loading.set(true);
    this.reviewService.getReviews(this.hubId(), 1).subscribe({
      next: (res: PaginatedReviews) => {
        this.reviews.set(res.items);
        this.hasMore.set(res.hasMoreItems);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadRating(): void {
    this.reviewService.getHubRating(this.hubId()).subscribe({
      next: (rating: HubRating) => this.hubRating.set(rating),
    });
  }

  private resetForm(): void {
    this.editingReview.set(null);
    this.formRating.set(0);
    this.reviewForm.reset();
    this.showForm.set(false);
  }
}
