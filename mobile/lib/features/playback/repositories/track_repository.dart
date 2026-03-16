import '../../../shared/api/bff_api_client.dart';
import '../models/now_playing_models.dart';
import '../models/search_models.dart';

abstract class NowPlayingRepository {
  Future<NowPlaying?> getNowPlaying(String hubId);
  Future<void> skipTrack(String hubId);
}

class NowPlayingRepositoryImpl implements NowPlayingRepository {
  NowPlayingRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<NowPlaying?> getNowPlaying(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/now-playing');
    if (response.statusCode == 204 || response.data == null) {
      return null;
    }
    return NowPlaying.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> skipTrack(String hubId) async {
    await _apiClient.post('/hubs/$hubId/queue/skip');
  }
}

abstract class TrackSearchRepository {
  Future<List<TrackSearchResult>> searchTracks(
    String query, {
    String? provider,
    int limit,
  });
  Future<void> proposeTrack(String hubId, ProposeTrackRequest request);
}

class TrackSearchRepositoryImpl implements TrackSearchRepository {
  TrackSearchRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<TrackSearchResult>> searchTracks(
    String query, {
    String? provider,
    int limit = 20,
  }) async {
    final queryParams = <String, dynamic>{
      'q': query,
      'limit': limit,
    };
    if (provider != null) {
      queryParams['provider'] = provider;
    }

    final response = await _apiClient.get(
      '/tracks/search',
      queryParameters: queryParams,
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => TrackSearchResult.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<void> proposeTrack(
    String hubId,
    ProposeTrackRequest request,
  ) async {
    await _apiClient.post(
      '/hubs/$hubId/queue/propose',
      data: {
        'track_id': request.trackId,
        'source': request.source,
      },
    );
  }
}
