import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'purchase_event.freezed.dart';

@freezed
class PurchaseEvent with _$PurchaseEvent {
  const factory PurchaseEvent.loadPackages() = PurchaseEventLoadPackages;
  const factory PurchaseEvent.purchasePackage({
    required CoinPackage package,
  }) = PurchaseEventPurchasePackage;
  const factory PurchaseEvent.verifyPurchase({
    required String platform,
    required String receiptData,
    required String userId,
    String? productId,
  }) = PurchaseEventVerifyPurchase;
}
