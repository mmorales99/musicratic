import 'package:freezed_annotation/freezed_annotation.dart';

import 'review_models.dart';

part 'reviews_event.freezed.dart';

@freezed
class ReviewsEvent with _$ReviewsEvent {
  const factory ReviewsEvent.load({required String hubId}) = ReviewsEventLoad;
  const factory ReviewsEvent.loadMore({required String hubId}) =
      ReviewsEventLoadMore;
  const factory ReviewsEvent.submit({
    required String hubId,
    required CreateReviewRequest request,
  }) = ReviewsEventSubmit;
  const factory ReviewsEvent.delete({
    required String hubId,
    required String reviewId,
  }) = ReviewsEventDelete;
}
