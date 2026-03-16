import '../../shared/api/bff_api_client.dart';

abstract class ProfileRepository {
  Future<Map<String, dynamic>> getMyProfile();
  Future<void> updateProfile(Map<String, dynamic> fields);
  Future<Map<String, dynamic>> getPublicProfile(String userId);
}

class ProfileRepositoryImpl implements ProfileRepository {
  ProfileRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<Map<String, dynamic>> getMyProfile() async {
    final response = await _apiClient.get('/profile/me');
    return response.data as Map<String, dynamic>;
  }

  @override
  Future<void> updateProfile(Map<String, dynamic> fields) async {
    await _apiClient.put('/profile/me', data: fields);
  }

  @override
  Future<Map<String, dynamic>> getPublicProfile(String userId) async {
    final response = await _apiClient.get('/profiles/$userId');
    return response.data as Map<String, dynamic>;
  }
}
