import 'package:freezed_annotation/freezed_annotation.dart';

part 'economy_models.freezed.dart';
part 'economy_models.g.dart';

@freezed
class Wallet with _$Wallet {
  const factory Wallet({
    required int balance,
    @Default('coins') String currency,
    @JsonKey(name: 'user_id') required String userId,
  }) = _Wallet;

  factory Wallet.fromJson(Map<String, dynamic> json) =>
      _$WalletFromJson(json);
}

enum TransactionType {
  @JsonValue('credit')
  credit,
  @JsonValue('debit')
  debit,
  @JsonValue('refund')
  refund,
  @JsonValue('purchase')
  purchase,
  @JsonValue('reward')
  reward,
}

@freezed
class Transaction with _$Transaction {
  const factory Transaction({
    required String id,
    required int amount,
    required TransactionType type,
    required String reason,
    @JsonKey(name: 'reference_id') String? referenceId,
    @JsonKey(name: 'created_at') required DateTime createdAt,
  }) = _Transaction;

  factory Transaction.fromJson(Map<String, dynamic> json) =>
      _$TransactionFromJson(json);
}

@freezed
class CoinPackage with _$CoinPackage {
  const factory CoinPackage({
    required String id,
    required String name,
    @JsonKey(name: 'coin_amount') required int coinAmount,
    @JsonKey(name: 'price_usd') required double priceUsd,
    @JsonKey(name: 'bonus_coins') @Default(0) int bonusCoins,
    @JsonKey(name: 'is_active') @Default(true) bool isActive,
  }) = _CoinPackage;

  factory CoinPackage.fromJson(Map<String, dynamic> json) =>
      _$CoinPackageFromJson(json);
}

enum SubscriptionTier {
  @JsonValue('free_trial')
  freeTrial,
  @JsonValue('monthly')
  monthly,
  @JsonValue('annual')
  annual,
  @JsonValue('event')
  event,
}

@freezed
class Subscription with _$Subscription {
  const factory Subscription({
    @JsonKey(name: 'hub_id') required String hubId,
    required SubscriptionTier tier,
    @JsonKey(name: 'started_at') required DateTime startedAt,
    @JsonKey(name: 'expires_at') required DateTime expiresAt,
    @JsonKey(name: 'trial_ends_at') DateTime? trialEndsAt,
    @JsonKey(name: 'is_active') @Default(true) bool isActive,
  }) = _Subscription;

  factory Subscription.fromJson(Map<String, dynamic> json) =>
      _$SubscriptionFromJson(json);
}

@freezed
class TrackPrice with _$TrackPrice {
  const factory TrackPrice({
    @JsonKey(name: 'base_cost') required int baseCost,
    @JsonKey(name: 'hotness_multiplier') required double hotnessMultiplier,
    @JsonKey(name: 'final_cost') required int finalCost,
  }) = _TrackPrice;

  factory TrackPrice.fromJson(Map<String, dynamic> json) =>
      _$TrackPriceFromJson(json);
}
