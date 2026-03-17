import 'package:flutter_bloc/flutter_bloc.dart';

import 'reviews_event.dart';
import 'reviews_repository.dart';
import 'reviews_state.dart';

class ReviewsBloc extends Bloc<ReviewsEvent, ReviewsState> {
  ReviewsBloc({required ReviewsRepository repository})
      : _repository = repository,
        super(const ReviewsState.loading()) {
    on<ReviewsEventLoad>(_onLoad);
    on<ReviewsEventLoadMore>(_onLoadMore);
    on<ReviewsEventSubmit>(_onSubmit);
    on<ReviewsEventDelete>(_onDelete);
  }

  final ReviewsRepository _repository;
  static const _pageSize = 20;

  Future<void> _onLoad(
    ReviewsEventLoad event,
    Emitter<ReviewsState> emit,
  ) async {
    emit(const ReviewsState.loading());
    try {
      final reviews = await _repository.getReviews(event.hubId);
      final rating = await _repository.getHubRating(event.hubId);
      emit(ReviewsState.loaded(
        reviews: reviews,
        rating: rating,
        hasMore: reviews.length >= _pageSize,
      ));
    } on Exception catch (e) {
      emit(ReviewsState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadMore(
    ReviewsEventLoadMore event,
    Emitter<ReviewsState> emit,
  ) async {
    final current = state;
    if (current is! ReviewsStateLoaded || !current.hasMore) return;

    try {
      final nextPage = current.currentPage + 1;
      final moreReviews = await _repository.getReviews(
        event.hubId,
        page: nextPage,
        pageSize: _pageSize,
      );
      emit(current.copyWith(
        reviews: [...current.reviews, ...moreReviews],
        currentPage: nextPage,
        hasMore: moreReviews.length >= _pageSize,
      ));
    } on Exception catch (e) {
      emit(ReviewsState.error(message: e.toString()));
    }
  }

  Future<void> _onSubmit(
    ReviewsEventSubmit event,
    Emitter<ReviewsState> emit,
  ) async {
    emit(const ReviewsState.submitting());
    try {
      await _repository.createReview(event.hubId, event.request);
      add(ReviewsEvent.load(hubId: event.hubId));
    } on Exception catch (e) {
      emit(ReviewsState.error(message: e.toString()));
    }
  }

  Future<void> _onDelete(
    ReviewsEventDelete event,
    Emitter<ReviewsState> emit,
  ) async {
    try {
      await _repository.deleteReview(event.hubId, event.reviewId);
      add(ReviewsEvent.load(hubId: event.hubId));
    } on Exception catch (e) {
      emit(ReviewsState.error(message: e.toString()));
    }
  }
}
