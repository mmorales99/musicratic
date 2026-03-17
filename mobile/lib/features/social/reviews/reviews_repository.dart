import '../../../shared/api/bff_api_client.dart';
import 'review_models.dart';

abstract class ReviewsRepository {
  Future<List<HubReview>> getReviews(String hubId, {int page, int pageSize});
  Future<HubRating> getHubRating(String hubId);
  Future<HubReview> createReview(String hubId, CreateReviewRequest request);
  Future<HubReview> updateReview(
    String hubId,
    String reviewId,
    CreateReviewRequest request,
  );
  Future<void> deleteReview(String hubId, String reviewId);
}

class ReviewsRepositoryImpl implements ReviewsRepository {
  ReviewsRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<HubReview>> getReviews(
    String hubId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    final response = await _apiClient.get(
      '/hubs/$hubId/reviews',
      queryParameters: {
        'page': page.toString(),
        'page_size': pageSize.toString(),
      },
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => HubReview.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<HubRating> getHubRating(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/rating');
    return HubRating.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<HubReview> createReview(
    String hubId,
    CreateReviewRequest request,
  ) async {
    final response = await _apiClient.post(
      '/hubs/$hubId/reviews',
      data: {'rating': request.rating, 'comment': request.comment},
    );
    return HubReview.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<HubReview> updateReview(
    String hubId,
    String reviewId,
    CreateReviewRequest request,
  ) async {
    final response = await _apiClient.put(
      '/hubs/$hubId/reviews/$reviewId',
      data: {'rating': request.rating, 'comment': request.comment},
    );
    return HubReview.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> deleteReview(String hubId, String reviewId) async {
    await _apiClient.delete('/hubs/$hubId/reviews/$reviewId');
  }
}
