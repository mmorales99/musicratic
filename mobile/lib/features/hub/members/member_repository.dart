import '../../../shared/api/bff_api_client.dart';
import 'member_models.dart';

abstract class MemberRepository {
  Future<List<HubMember>> getMembers(String hubId, {String? search});
  Future<void> removeMember(String hubId, String memberId);
}

class MemberRepositoryImpl implements MemberRepository {
  MemberRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<HubMember>> getMembers(
    String hubId, {
    String? search,
  }) async {
    final params = <String, dynamic>{};
    if (search != null && search.isNotEmpty) {
      params['search'] = search;
    }
    final response = await _apiClient.get(
      '/hubs/$hubId/members',
      queryParameters: params,
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => HubMember.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<void> removeMember(String hubId, String memberId) async {
    await _apiClient.delete('/hubs/$hubId/members/$memberId');
  }
}
