import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import {
  Review,
  HubRating,
  PaginatedReviews,
  CreateReviewRequest,
  UpdateReviewRequest,
} from "../models/review.model";

@Injectable({ providedIn: "root" })
export class ReviewService {
  private readonly api = inject(BffApiService);

  getReviews(hubId: string, page: number): Observable<PaginatedReviews> {
    const encodedId = encodeURIComponent(hubId);
    return this.api.get<PaginatedReviews>(
      `/social/hubs/${encodedId}/reviews?page=${page}`,
    );
  }

  createReview(
    hubId: string,
    rating: number,
    comment?: string,
  ): Observable<Review> {
    const body: CreateReviewRequest = { rating, comment };
    return this.api.post<Review>(
      `/social/hubs/${encodeURIComponent(hubId)}/reviews`,
      body,
    );
  }

  updateReview(
    reviewId: string,
    rating: number,
    comment?: string,
  ): Observable<Review> {
    const body: UpdateReviewRequest = { rating, comment };
    return this.api.put<Review>(
      `/social/reviews/${encodeURIComponent(reviewId)}`,
      body,
    );
  }

  deleteReview(reviewId: string): Observable<void> {
    return this.api.delete<void>(
      `/social/reviews/${encodeURIComponent(reviewId)}`,
    );
  }

  getHubRating(hubId: string): Observable<HubRating> {
    return this.api.get<HubRating>(
      `/social/hubs/${encodeURIComponent(hubId)}/rating`,
    );
  }
}
