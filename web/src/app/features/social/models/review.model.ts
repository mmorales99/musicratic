export interface Review {
  id: string;
  hubId: string;
  userId: string;
  reviewerName: string;
  reviewerAvatarUrl: string | null;
  rating: number;
  comment: string | null;
  ownerResponse: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface HubRating {
  hubId: string;
  averageRating: number;
  totalReviews: number;
  isVisible: boolean;
}

export interface CreateReviewRequest {
  rating: number;
  comment?: string;
}

export interface UpdateReviewRequest {
  rating: number;
  comment?: string;
}

export interface PaginatedReviews {
  success: boolean;
  totalItemsInResponse: number;
  hasMoreItems: boolean;
  items: Review[];
  audit: { requestId: string; timestamp: string };
}
