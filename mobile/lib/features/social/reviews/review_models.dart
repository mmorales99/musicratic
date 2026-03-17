import 'package:freezed_annotation/freezed_annotation.dart';

part 'review_models.freezed.dart';
part 'review_models.g.dart';

@freezed
class HubReview with _$HubReview {
  const factory HubReview({
    required String id,
    @JsonKey(name: 'hub_id') required String hubId,
    @JsonKey(name: 'user_id') required String userId,
    @JsonKey(name: 'display_name') required String displayName,
    String? avatar,
    required int rating,
    String? comment,
    @JsonKey(name: 'created_at') DateTime? createdAt,
  }) = _HubReview;

  factory HubReview.fromJson(Map<String, dynamic> json) =>
      _$HubReviewFromJson(json);
}

@freezed
class HubRating with _$HubRating {
  const factory HubRating({
    @JsonKey(name: 'average_rating') @Default(0.0) double averageRating,
    @JsonKey(name: 'total_reviews') @Default(0) int totalReviews,
    @JsonKey(name: 'rating_distribution')
    @Default({})
    Map<String, int> ratingDistribution,
  }) = _HubRating;

  factory HubRating.fromJson(Map<String, dynamic> json) =>
      _$HubRatingFromJson(json);
}

@freezed
class CreateReviewRequest with _$CreateReviewRequest {
  const factory CreateReviewRequest({
    required int rating,
    String? comment,
  }) = _CreateReviewRequest;

  factory CreateReviewRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateReviewRequestFromJson(json);
}
