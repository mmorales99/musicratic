import '../../../shared/api/bff_api_client.dart';

abstract class ShareService {
  Future<String> getShareLink(String hubId);
}

class ShareServiceImpl implements ShareService {
  ShareServiceImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<String> getShareLink(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/share-link');
    final data = response.data as Map<String, dynamic>;
    return data['link'] as String;
  }
}
