import '../../../shared/api/bff_api_client.dart';

abstract class RoleRepository {
  Future<void> promoteMember(String hubId, String memberId, String newRole);
  Future<void> demoteMember(String hubId, String memberId, String newRole);
  Future<Map<String, dynamic>> getTierLimits(String hubId);
}

class RoleRepositoryImpl implements RoleRepository {
  RoleRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<void> promoteMember(
    String hubId,
    String memberId,
    String newRole,
  ) async {
    await _apiClient.put(
      '/hubs/$hubId/members/$memberId/promote',
      data: {'role': newRole},
    );
  }

  @override
  Future<void> demoteMember(
    String hubId,
    String memberId,
    String newRole,
  ) async {
    await _apiClient.put(
      '/hubs/$hubId/members/$memberId/demote',
      data: {'role': newRole},
    );
  }

  @override
  Future<Map<String, dynamic>> getTierLimits(String hubId) async {
    final response = await _apiClient.get('/hubs/$hubId/tier-limits');
    return response.data as Map<String, dynamic>;
  }
}
