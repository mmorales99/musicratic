import '../../../shared/api/bff_api_client.dart';
import '../../../shared/models/hub.dart';
import '../../../shared/models/paged_result.dart';
import '../models/create_hub_request.dart';
import '../models/search_hubs_params.dart';

abstract class HubRepository {
  Future<List<Hub>> getActiveHubs();
  Future<Hub> getHub(String id);
  Future<Hub> createHub(CreateHubRequest request);
  Future<Hub> updateHub(String id, Map<String, dynamic> data);
  Future<void> deleteHub(String id);
  Future<HubSettings> getHubSettings(String id);
  Future<HubSettings> updateHubSettings(String id, Map<String, dynamic> data);
  Future<void> activateHub(String id);
  Future<void> deactivateHub(String id);
  Future<void> pauseHub(String id);
  Future<void> resumeHub(String id);
  Future<void> attachToHub(String hubId);
  Future<Hub> attachByCode(String code);
  Future<void> detachFromHub();
  Future<PagedResult<Hub>> searchHubs(SearchHubsParams params);
}

class HubRepositoryImpl implements HubRepository {
  HubRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<Hub>> getActiveHubs() async {
    final response = await _apiClient.get('/hubs');
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => Hub.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<Hub> getHub(String id) async {
    final response = await _apiClient.get('/hubs/$id');
    return Hub.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<Hub> createHub(CreateHubRequest request) async {
    final response = await _apiClient.post('/hubs', data: request.toJson());
    return Hub.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<Hub> updateHub(String id, Map<String, dynamic> data) async {
    final response = await _apiClient.put('/hubs/$id', data: data);
    return Hub.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> deleteHub(String id) async {
    await _apiClient.delete('/hubs/$id');
  }

  @override
  Future<HubSettings> getHubSettings(String id) async {
    final response = await _apiClient.get('/hubs/$id/settings');
    return HubSettings.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<HubSettings> updateHubSettings(
    String id,
    Map<String, dynamic> data,
  ) async {
    final response = await _apiClient.put('/hubs/$id/settings', data: data);
    return HubSettings.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> activateHub(String id) async {
    await _apiClient.post('/hubs/$id/activate');
  }

  @override
  Future<void> deactivateHub(String id) async {
    await _apiClient.post('/hubs/$id/deactivate');
  }

  @override
  Future<void> pauseHub(String id) async {
    await _apiClient.post('/hubs/$id/pause');
  }

  @override
  Future<void> resumeHub(String id) async {
    await _apiClient.post('/hubs/$id/resume');
  }

  @override
  Future<void> attachToHub(String hubId) async {
    await _apiClient.post('/hubs/$hubId/attach');
  }

  @override
  Future<Hub> attachByCode(String code) async {
    final response = await _apiClient.post(
      '/hubs/attach',
      data: {'code': code},
    );
    return Hub.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> detachFromHub() async {
    await _apiClient.post('/hubs/detach');
  }

  @override
  Future<PagedResult<Hub>> searchHubs(SearchHubsParams params) async {
    final response = await _apiClient.get(
      '/hubs/search',
      queryParameters: params.toQueryParameters(),
    );
    final data = response.data as Map<String, dynamic>;
    return PagedResult.fromJson(
      data,
      (json) => Hub.fromJson(json! as Map<String, dynamic>),
    );
  }
}
