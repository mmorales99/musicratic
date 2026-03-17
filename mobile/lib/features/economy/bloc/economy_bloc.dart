import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/economy_models.dart';
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
      final wallet = await _repository.getWallet();
      emit(EconomyState.walletLoaded(balance: wallet.balance));
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
      // Purchase flow now handled by PurchaseBloc; this is a legacy
      // convenience that reloads the wallet balance.
      final wallet = await _repository.getWallet();
      emit(EconomyState.walletLoaded(balance: wallet.balance));
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
      final transactions = await _repository.getTransactions();
      emit(EconomyState.transactionsLoaded(transactions: transactions));
    } on Exception catch (e) {
      emit(EconomyState.error(message: e.toString()));
    }
  }
}
