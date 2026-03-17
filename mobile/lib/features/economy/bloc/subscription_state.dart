import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'subscription_state.freezed.dart';

@freezed
class SubscriptionState with _$SubscriptionState {
  const factory SubscriptionState.initial() = SubscriptionStateInitial;
  const factory SubscriptionState.loading() = SubscriptionStateLoading;
  const factory SubscriptionState.loaded({
    required Subscription subscription,
  }) = SubscriptionStateLoaded;
  const factory SubscriptionState.noSubscription({
    required String hubId,
  }) = SubscriptionStateNoSubscription;
  const factory SubscriptionState.error({required String message}) =
      SubscriptionStateError;
}
