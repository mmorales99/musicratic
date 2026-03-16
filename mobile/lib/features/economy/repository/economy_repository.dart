import '../../shared/api/bff_api_client.dart';

abstract class EconomyRepository {
  Future<int> getWalletBalance();
  Future<void> purchaseCoins({required int amount});
  Future<List<dynamic>> getTransactionHistory();
}

class EconomyRepositoryImpl implements EconomyRepository {
  EconomyRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<int> getWalletBalance() async {
    final response = await _apiClient.get('/wallet/balance');
    final data = response.data as Map<String, dynamic>;
    return data['balance'] as int;
  }

  @override
  Future<void> purchaseCoins({required int amount}) async {
    await _apiClient.post('/wallet/purchase', data: {'amount': amount});
  }

  @override
  Future<List<dynamic>> getTransactionHistory() async {
    final response = await _apiClient.get('/wallet/transactions');
    final data = response.data as Map<String, dynamic>;
    return data['items'] as List<dynamic>;
  }
}
