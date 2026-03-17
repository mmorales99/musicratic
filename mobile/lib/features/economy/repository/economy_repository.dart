import '../models/economy_models.dart';
import '../../../shared/api/bff_api_client.dart';

abstract class EconomyRepository {
  Future<Wallet> getWallet();
  Future<List<Transaction>> getTransactions({
    int page = 1,
    int pageSize = 20,
    TransactionType? type,
  });
  Future<List<CoinPackage>> getCoinPackages();
  Future<Subscription> getSubscription(String hubId);
  Future<TrackPrice> getTrackPrice({
    required int durationSeconds,
    required double hotness,
  });
  Future<Wallet> verifyAppleReceipt({
    required String receiptData,
    required String userId,
  });
  Future<Wallet> verifyGooglePurchase({
    required String purchaseToken,
    required String productId,
    required String userId,
  });
  Future<Subscription> startTrial(String hubId);
}

class EconomyRepositoryImpl implements EconomyRepository {
  EconomyRepositoryImpl({required BffApiClient apiClient})
      : _apiClient = apiClient;

  final BffApiClient _apiClient;

  @override
  Future<Wallet> getWallet() async {
    final response = await _apiClient.get('/wallet');
    return Wallet.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<List<Transaction>> getTransactions({
    int page = 1,
    int pageSize = 20,
    TransactionType? type,
  }) async {
    final queryParams = <String, dynamic>{
      'page': page,
      'page_size': pageSize,
    };
    if (type != null) {
      queryParams['type'] = type.name;
    }
    final response = await _apiClient.get(
      '/wallet/transactions',
      queryParameters: queryParams,
    );
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => Transaction.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<List<CoinPackage>> getCoinPackages() async {
    final response = await _apiClient.get('/coin-packages');
    final data = response.data as Map<String, dynamic>;
    final items = data['items'] as List<dynamic>;
    return items
        .map((e) => CoinPackage.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<Subscription> getSubscription(String hubId) async {
    final response = await _apiClient.get('/subscriptions/$hubId');
    return Subscription.fromJson(
      response.data as Map<String, dynamic>,
    );
  }

  @override
  Future<TrackPrice> getTrackPrice({
    required int durationSeconds,
    required double hotness,
  }) async {
    final response = await _apiClient.get(
      '/track-price',
      queryParameters: {
        'duration_seconds': durationSeconds,
        'hotness': hotness,
      },
    );
    return TrackPrice.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<Wallet> verifyAppleReceipt({
    required String receiptData,
    required String userId,
  }) async {
    final response = await _apiClient.post(
      '/purchases/verify-apple',
      data: {
        'receipt_data': receiptData,
        'user_id': userId,
      },
    );
    return Wallet.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<Wallet> verifyGooglePurchase({
    required String purchaseToken,
    required String productId,
    required String userId,
  }) async {
    final response = await _apiClient.post(
      '/purchases/verify-google',
      data: {
        'purchase_token': purchaseToken,
        'product_id': productId,
        'user_id': userId,
      },
    );
    return Wallet.fromJson(response.data as Map<String, dynamic>);
  }

  @override
  Future<Subscription> startTrial(String hubId) async {
    final response = await _apiClient.post(
      '/subscriptions/$hubId/trial',
    );
    return Subscription.fromJson(
      response.data as Map<String, dynamic>,
    );
  }
}
