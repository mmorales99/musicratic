import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'wallet_state.freezed.dart';

@freezed
class WalletState with _$WalletState {
  const factory WalletState.initial() = WalletStateInitial;
  const factory WalletState.loading() = WalletStateLoading;
  const factory WalletState.loaded({
    required Wallet wallet,
    required List<Transaction> transactions,
    @Default(false) bool hasMore,
    @Default(1) int currentPage,
    TransactionType? activeFilter,
    @Default(false) bool isLoadingMore,
  }) = WalletStateLoaded;
  const factory WalletState.error({required String message}) =
      WalletStateError;
}
