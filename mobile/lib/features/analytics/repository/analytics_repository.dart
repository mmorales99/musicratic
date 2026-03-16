import '../../shared/api/bff_api_client.dart';

abstract class AnalyticsRepository {
  Future<Map<String, dynamic>> getWeeklyReport(String hubId);
  Future<Map<String, dynamic>> getMonthlyReport(String hubId);
  Future<Map<String, dynamic>> getTrackStats(String trackId);
}

class AnalyticsRepositoryImpl implements AnalyticsRepository {
  AnalyticsRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<Map<String, dynamic>> getWeeklyReport(String hubId) async {
    final response = await _apiClient.get('/analytics/$hubId/weekly');
    return response.data as Map<String, dynamic>;
  }

  @override
  Future<Map<String, dynamic>> getMonthlyReport(String hubId) async {
    final response = await _apiClient.get('/analytics/$hubId/monthly');
    return response.data as Map<String, dynamic>;
  }

  @override
  Future<Map<String, dynamic>> getTrackStats(String trackId) async {
    final response = await _apiClient.get('/analytics/tracks/$trackId');
    return response.data as Map<String, dynamic>;
  }
}
