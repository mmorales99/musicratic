import '../../../shared/api/bff_api_client.dart';
import 'report_models.dart';

abstract class ReportsRepository {
  Future<List<AnalyticsReport>> getReports(String hubId, {String period});
}

class ReportsRepositoryImpl implements ReportsRepository {
  ReportsRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<List<AnalyticsReport>> getReports(
    String hubId, {
    String period = 'weekly',
  }) async {
    final response = await _apiClient.get(
      '/api/analytics/hubs/$hubId/reports',
      queryParameters: {'period': period},
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => AnalyticsReport.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
