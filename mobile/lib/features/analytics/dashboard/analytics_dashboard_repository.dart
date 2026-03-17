import '../../../shared/api/bff_api_client.dart';
import 'dashboard_models.dart';

abstract class DashboardRepository {
  Future<DashboardData> getDashboard(String hubId);
}

class DashboardRepositoryImpl implements DashboardRepository {
  DashboardRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<DashboardData> getDashboard(String hubId) async {
    final response =
        await _apiClient.get('/api/analytics/hubs/$hubId/dashboard');
    return DashboardData.fromJson(response.data as Map<String, dynamic>);
  }
}
