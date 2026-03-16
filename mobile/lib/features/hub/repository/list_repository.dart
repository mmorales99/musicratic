import '../../../shared/api/bff_api_client.dart';
import '../models/list_models.dart';

abstract class ListRepository {
  Future<List<HubList>> getLists(String hubId);
  Future<HubList> getList(String listId);
  Future<HubList> createList(String hubId, CreateListRequest request);
  Future<HubList> updateList(String listId, Map<String, dynamic> data);
  Future<void> deleteList(String listId);
  Future<List<ListTrack>> getListTracks(String listId);
  Future<ListTrack> addTrack(String listId, {required String trackId});
  Future<void> removeTrack(String listId, String trackId);
  Future<void> reorderTracks(String listId, List<String> trackIds);
}

class ListRepositoryImpl implements ListRepository {
  ListRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<HubList>> getLists(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/lists');
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => HubList.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<HubList> getList(String listId) async {
    final response = await _apiClient.get('/lists/$listId');
    return HubList.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<HubList> createList(String hubId, CreateListRequest request) async {
    final response = await _apiClient.post(
      '/hubs/$hubId/lists',
      data: request.toJson(),
    );
    return HubList.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<HubList> updateList(String listId, Map<String, dynamic> data) async {
    final response = await _apiClient.put('/lists/$listId', data: data);
    return HubList.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> deleteList(String listId) async {
    await _apiClient.delete('/lists/$listId');
  }

  @override
  Future<List<ListTrack>> getListTracks(String listId) async {
    final response = await _apiClient.get('/lists/$listId/tracks');
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => ListTrack.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<ListTrack> addTrack(String listId, {required String trackId}) async {
    final response = await _apiClient.post(
      '/lists/$listId/tracks',
      data: {'track_id': trackId},
    );
    return ListTrack.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<void> removeTrack(String listId, String trackId) async {
    await _apiClient.delete('/lists/$listId/tracks/$trackId');
  }

  @override
  Future<void> reorderTracks(String listId, List<String> trackIds) async {
    await _apiClient.put(
      '/lists/$listId/tracks/reorder',
      data: {'track_ids': trackIds},
    );
  }
}
