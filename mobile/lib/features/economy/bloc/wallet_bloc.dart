import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/economy_models.dart';
import '../repository/economy_repository.dart';
import 'wallet_event.dart';
import 'wallet_state.dart';

class WalletBloc extends Bloc<WalletEvent, WalletState> {
  WalletBloc({required EconomyRepository repository})
      : _repository = repository,
        super(const WalletState.initial()) {
    on<WalletEventLoadWallet>(_onLoadWallet);
    on<WalletEventLoadTransactions>(_onLoadTransactions);
    on<WalletEventFilterByType>(_onFilterByType);
    on<WalletEventLoadMore>(_onLoadMore);
  }

  final EconomyRepository _repository;
  static const int _pageSize = 20;

  Future<void> _onLoadWallet(
    WalletEventLoadWallet event,
    Emitter<WalletState> emit,
  ) async {
    emit(const WalletState.loading());
    try {
      final wallet = await _repository.getWallet();
      final transactions = await _repository.getTransactions(
        page: 1,
        pageSize: _pageSize,
      );
      emit(WalletState.loaded(
        wallet: wallet,
        transactions: transactions,
        hasMore: transactions.length >= _pageSize,
        currentPage: 1,
      ));
    } on Exception catch (e) {
      emit(WalletState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadTransactions(
    WalletEventLoadTransactions event,
    Emitter<WalletState> emit,
  ) async {
    final current = state;
    if (current is! WalletStateLoaded) return;

    emit(const WalletState.loading());
    try {
      final transactions = await _repository.getTransactions(
        page: 1,
        pageSize: _pageSize,
        type: current.activeFilter,
      );
      final wallet = await _repository.getWallet();
      emit(WalletState.loaded(
        wallet: wallet,
        transactions: transactions,
        hasMore: transactions.length >= _pageSize,
        currentPage: 1,
        activeFilter: current.activeFilter,
      ));
    } on Exception catch (e) {
      emit(WalletState.error(message: e.toString()));
    }
  }

  Future<void> _onFilterByType(
    WalletEventFilterByType event,
    Emitter<WalletState> emit,
  ) async {
    final current = state;
    Wallet wallet;

    if (current is WalletStateLoaded) {
      wallet = current.wallet;
    } else {
      emit(const WalletState.loading());
      try {
        wallet = await _repository.getWallet();
      } on Exception catch (e) {
        emit(WalletState.error(message: e.toString()));
        return;
      }
    }

    try {
      final transactions = await _repository.getTransactions(
        page: 1,
        pageSize: _pageSize,
        type: event.type,
      );
      emit(WalletState.loaded(
        wallet: wallet,
        transactions: transactions,
        hasMore: transactions.length >= _pageSize,
        currentPage: 1,
        activeFilter: event.type,
      ));
    } on Exception catch (e) {
      emit(WalletState.error(message: e.toString()));
    }
  }

  Future<void> _onLoadMore(
    WalletEventLoadMore event,
    Emitter<WalletState> emit,
  ) async {
    final current = state;
    if (current is! WalletStateLoaded || current.isLoadingMore) return;
    if (!current.hasMore) return;

    emit(current.copyWith(isLoadingMore: true));

    try {
      final nextPage = current.currentPage + 1;
      final moreTransactions = await _repository.getTransactions(
        page: nextPage,
        pageSize: _pageSize,
        type: current.activeFilter,
      );
      emit(current.copyWith(
        transactions: [...current.transactions, ...moreTransactions],
        currentPage: nextPage,
        hasMore: moreTransactions.length >= _pageSize,
        isLoadingMore: false,
      ));
    } on Exception catch (e) {
      emit(current.copyWith(isLoadingMore: false));
      emit(WalletState.error(message: e.toString()));
    }
  }
}
