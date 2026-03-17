import 'dart:io';

import 'package:flutter_bloc/flutter_bloc.dart';

import '../models/economy_models.dart';
import '../repository/economy_repository.dart';
import 'purchase_event.dart';
import 'purchase_state.dart';

class PurchaseBloc extends Bloc<PurchaseEvent, PurchaseState> {
  PurchaseBloc({required EconomyRepository repository})
      : _repository = repository,
        super(const PurchaseState.initial()) {
    on<PurchaseEventLoadPackages>(_onLoadPackages);
    on<PurchaseEventPurchasePackage>(_onPurchasePackage);
    on<PurchaseEventVerifyPurchase>(_onVerifyPurchase);
  }

  final EconomyRepository _repository;

  Future<void> _onLoadPackages(
    PurchaseEventLoadPackages event,
    Emitter<PurchaseState> emit,
  ) async {
    emit(const PurchaseState.loading());
    try {
      final packages = await _repository.getCoinPackages();
      final activePackages =
          packages.where((p) => p.isActive).toList();
      emit(PurchaseState.packagesLoaded(packages: activePackages));
    } on Exception catch (e) {
      emit(PurchaseState.error(message: e.toString()));
    }
  }

  Future<void> _onPurchasePackage(
    PurchaseEventPurchasePackage event,
    Emitter<PurchaseState> emit,
  ) async {
    final currentPackages = _currentPackages;
    if (currentPackages == null) return;

    emit(PurchaseState.purchasing(
      packages: currentPackages,
      packageId: event.package.id,
    ));

    // In a real implementation, this would trigger the native IAP flow
    // via in_app_purchase package. For now, we emit selecting state
    // and wait for verifyPurchase to be dispatched after IAP completes.
  }

  Future<void> _onVerifyPurchase(
    PurchaseEventVerifyPurchase event,
    Emitter<PurchaseState> emit,
  ) async {
    final currentPackages = _currentPackages;

    try {
      Wallet wallet;
      if (event.platform == 'apple' || Platform.isIOS) {
        wallet = await _repository.verifyAppleReceipt(
          receiptData: event.receiptData,
          userId: event.userId,
        );
      } else {
        wallet = await _repository.verifyGooglePurchase(
          purchaseToken: event.receiptData,
          productId: event.productId ?? '',
          userId: event.userId,
        );
      }
      emit(PurchaseState.success(
        updatedWallet: wallet,
        packages: currentPackages ?? [],
      ));
    } on Exception catch (e) {
      emit(PurchaseState.error(
        message: e.toString(),
        packages: currentPackages,
      ));
    }
  }

  List<CoinPackage>? get _currentPackages {
    return state.when(
      initial: () => null,
      loading: () => null,
      packagesLoaded: (packages) => packages,
      purchasing: (packages, _) => packages,
      success: (_, packages) => packages,
      error: (_, packages) => packages,
    );
  }
}
