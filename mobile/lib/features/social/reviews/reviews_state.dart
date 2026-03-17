import 'package:freezed_annotation/freezed_annotation.dart';

import 'review_models.dart';

part 'reviews_state.freezed.dart';

@freezed
class ReviewsState with _$ReviewsState {
  const factory ReviewsState.loading() = ReviewsStateLoading;
  const factory ReviewsState.loaded({
    required List<HubReview> reviews,
    required HubRating rating,
    @Default(false) bool hasMore,
    @Default(1) int currentPage,
  }) = ReviewsStateLoaded;
  const factory ReviewsState.submitting() = ReviewsStateSubmitting;
  const factory ReviewsState.error({required String message}) =
      ReviewsStateError;
}
