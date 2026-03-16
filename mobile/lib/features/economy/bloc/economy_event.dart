import 'package:freezed_annotation/freezed_annotation.dart';

part 'economy_event.freezed.dart';

@freezed
class EconomyEvent with _$EconomyEvent {
  const factory EconomyEvent.loadWallet() = EconomyEventLoadWallet;
  const factory EconomyEvent.purchaseCoins({required int amount}) =
      EconomyEventPurchaseCoins;
  const factory EconomyEvent.loadTransactions() =
      EconomyEventLoadTransactions;
}
