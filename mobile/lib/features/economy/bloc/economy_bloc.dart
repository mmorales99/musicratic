import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/economy_repository.dart';
import 'economy_event.dart';
import 'economy_state.dart';

class EconomyBloc extends Bloc<EconomyEvent, EconomyState> {
  EconomyBloc({required EconomyRepository repository})
      : _repository = repository,
        super(const EconomyState.initial()) {
    on<EconomyEventLoadWallet>(_onLoadWallet);
    on<EconomyEventPurchaseCoins>(_onPurchaseCoins);
    on<EconomyEventLoadTransactions>(_onLoadTransactions);
  }

  final EconomyRepository _repository;

  Future<void> _onLoadWallet(
    EconomyEventLoadWallet event,
    Emitter<EconomyState> emit,
  ) async {
    emit(const EconomyState.loading());
    try {
      final balance = await _repository.getWalletBalance();
      emit(EconomyState.walletLoaded(balance: balance));
    } on Exception catch (e) {
      emit(EconomyState.error(message: e.toString()));
    }
  }

  Future<void> _onPurchaseCoins(
    EconomyEventPurchaseCoins event,
    Emitter<EconomyState> emit,
  ) async {
    emit(const EconomyState.loading());
    try {
      await _repository.purchaseCoins(amount: event.amount);
      final balance = await _repository.getWalletBalance();
      emit(EconomyState.walletLoaded(balance: balance));
    } on Exception catch (e) {
      emit(EconomyState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadTransactions(
    EconomyEventLoadTransactions event,
    Emitter<EconomyState> emit,
  ) async {
    emit(const EconomyState.loading());
    try {
      final transactions = await _repository.getTransactionHistory();
      emit(EconomyState.transactionsLoaded(transactions: transactions));
    } on Exception catch (e) {
      emit(EconomyState.error(message: e.toString()));
    }
  }
}
