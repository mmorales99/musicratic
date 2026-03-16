import '../../shared/api/bff_api_client.dart';

abstract class HubRepository {
  Future<List<dynamic>> getActiveHubs();
  Future<void> attachToHub(String hubId);
  Future<void> detachFromHub();
}

class HubRepositoryImpl implements HubRepository {
  HubRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<dynamic>> getActiveHubs() async {
    final response = await _apiClient.get('/hubs');
    final data = response.data as Map<String, dynamic>;
    return data['items'] as List<dynamic>;
  }

  @override
  Future<void> attachToHub(String hubId) async {
    await _apiClient.post('/hubs/$hubId/attach');
  }

  @override
  Future<void> detachFromHub() async {
    await _apiClient.post('/hubs/detach');
  }
}
