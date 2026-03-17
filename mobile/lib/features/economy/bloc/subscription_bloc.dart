import 'package:dio/dio.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../repository/economy_repository.dart';
import 'subscription_event.dart';
import 'subscription_state.dart';

class SubscriptionBloc
    extends Bloc<SubscriptionEvent, SubscriptionState> {
  SubscriptionBloc({required EconomyRepository repository})
      : _repository = repository,
        super(const SubscriptionState.initial()) {
    on<SubscriptionEventLoadSubscription>(_onLoadSubscription);
    on<SubscriptionEventStartTrial>(_onStartTrial);
    on<SubscriptionEventUpgradeTier>(_onUpgradeTier);
  }

  final EconomyRepository _repository;

  Future<void> _onLoadSubscription(
    SubscriptionEventLoadSubscription event,
    Emitter<SubscriptionState> emit,
  ) async {
    emit(const SubscriptionState.loading());
    try {
      final subscription =
          await _repository.getSubscription(event.hubId);
      emit(SubscriptionState.loaded(subscription: subscription));
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        emit(SubscriptionState.noSubscription(hubId: event.hubId));
      } else {
        emit(SubscriptionState.error(message: e.toString()));
      }
    } on Exception catch (e) {
      emit(SubscriptionState.error(message: e.toString()));
    }
  }

  Future<void> _onStartTrial(
    SubscriptionEventStartTrial event,
    Emitter<SubscriptionState> emit,
  ) async {
    emit(const SubscriptionState.loading());
    try {
      final subscription =
          await _repository.startTrial(event.hubId);
      emit(SubscriptionState.loaded(subscription: subscription));
    } on Exception catch (e) {
      emit(SubscriptionState.error(message: e.toString()));
    }
  }

  Future<void> _onUpgradeTier(
    SubscriptionEventUpgradeTier event,
    Emitter<SubscriptionState> emit,
  ) async {
    // Upgrade tier triggers the purchase flow via IAP.
    // After successful IAP, the backend verifies and updates the
    // subscription. We then reload subscription state.
    emit(const SubscriptionState.loading());
    try {
      final subscription =
          await _repository.getSubscription(event.hubId);
      emit(SubscriptionState.loaded(subscription: subscription));
    } on Exception catch (e) {
      emit(SubscriptionState.error(message: e.toString()));
    }
  }
}
