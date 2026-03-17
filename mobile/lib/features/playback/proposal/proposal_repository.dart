import '../../../shared/api/bff_api_client.dart';
import '../../../shared/models/track.dart';

abstract class ProposalRepository {
  Future<QueueEntry> proposeTrack(
    String hubId,
    String trackId,
    String providerId,
  );
  Future<int> getCoinBalance();
  Future<int> getTrackCost(String trackId, String providerId);
}

class ProposalRepositoryImpl implements ProposalRepository {
  ProposalRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<QueueEntry> proposeTrack(
    String hubId,
    String trackId,
    String providerId,
  ) async {
    final response = await _apiClient.post(
      '/hubs/$hubId/queue/propose',
      data: {
        'track_id': trackId,
        'provider_id': providerId,
      },
    );
    return QueueEntry.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<int> getCoinBalance() async {
    final response = await _apiClient.get('/wallet');
    final data = response.data as Map<String, dynamic>;
    return data['balance'] as int;
  }

  @override
  Future<int> getTrackCost(String trackId, String providerId) async {
    final response = await _apiClient.get(
      '/tracks/$trackId/cost',
      queryParameters: {'provider_id': providerId},
    );
    final data = response.data as Map<String, dynamic>;
    return data['final_cost'] as int;
  }
}
