import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'wallet_event.freezed.dart';

@freezed
class WalletEvent with _$WalletEvent {
  const factory WalletEvent.loadWallet() = WalletEventLoadWallet;
  const factory WalletEvent.loadTransactions() =
      WalletEventLoadTransactions;
  const factory WalletEvent.filterByType({TransactionType? type}) =
      WalletEventFilterByType;
  const factory WalletEvent.loadMore() = WalletEventLoadMore;
}
