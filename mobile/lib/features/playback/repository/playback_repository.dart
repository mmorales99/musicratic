import '../../shared/api/bff_api_client.dart';

abstract class PlaybackRepository {
  Future<List<dynamic>> getQueue(String hubId);
  Future<void> skipTrack(String trackId);
  Future<void> proposeTrack({
    required String hubId,
    required String trackUri,
  });
}

class PlaybackRepositoryImpl implements PlaybackRepository {
  PlaybackRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<dynamic>> getQueue(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/queue');
    final data = response.data as Map<String, dynamic>;
    return data['items'] as List<dynamic>;
  }

  @override
  Future<void> skipTrack(String trackId) async {
    await _apiClient.post('/playback/skip', data: {'trackId': trackId});
  }

  @override
  Future<void> proposeTrack({
    required String hubId,
    required String trackUri,
  }) async {
    await _apiClient.post(
      '/hubs/$hubId/queue',
      data: {'trackUri': trackUri},
    );
  }
}
