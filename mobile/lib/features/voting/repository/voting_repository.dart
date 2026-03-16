import '../../shared/api/bff_api_client.dart';

abstract class VotingRepository {
  Future<void> castVote({
    required String queueEntryId,
    required String value,
  });
  Future<Map<String, dynamic>> getTally(String queueEntryId);
}

class VotingRepositoryImpl implements VotingRepository {
  VotingRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<void> castVote({
    required String queueEntryId,
    required String value,
  }) async {
    await _apiClient.post(
      '/votes',
      data: {
        'queueEntryId': queueEntryId,
        'value': value,
      },
    );
  }

  @override
  Future<Map<String, dynamic>> getTally(String queueEntryId) async {
    final response = await _apiClient.get('/votes/$queueEntryId/tally');
    return response.data as Map<String, dynamic>;
  }
}
