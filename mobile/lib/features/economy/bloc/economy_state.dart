import 'package:freezed_annotation/freezed_annotation.dart';

part 'economy_state.freezed.dart';

@freezed
class EconomyState with _$EconomyState {
  const factory EconomyState.initial() = EconomyStateInitial;
  const factory EconomyState.loading() = EconomyStateLoading;
  const factory EconomyState.walletLoaded({required int balance}) =
      EconomyStateWalletLoaded;
  const factory EconomyState.transactionsLoaded({
    required List<dynamic> transactions,
  }) = EconomyStateTransactionsLoaded;
  const factory EconomyState.error({required String message}) =
      EconomyStateError;
}
