import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'purchase_state.freezed.dart';

@freezed
class PurchaseState with _$PurchaseState {
  const factory PurchaseState.initial() = PurchaseStateInitial;
  const factory PurchaseState.loading() = PurchaseStateLoading;
  const factory PurchaseState.packagesLoaded({
    required List<CoinPackage> packages,
  }) = PurchaseStatePackagesLoaded;
  const factory PurchaseState.purchasing({
    required List<CoinPackage> packages,
    required String packageId,
  }) = PurchaseStatePurchasing;
  const factory PurchaseState.success({
    required Wallet updatedWallet,
    required List<CoinPackage> packages,
  }) = PurchaseStateSuccess;
  const factory PurchaseState.error({
    required String message,
    List<CoinPackage>? packages,
  }) = PurchaseStateError;
}
