import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/economy_models.dart';

part 'subscription_event.freezed.dart';

@freezed
class SubscriptionEvent with _$SubscriptionEvent {
  const factory SubscriptionEvent.loadSubscription({
    required String hubId,
  }) = SubscriptionEventLoadSubscription;
  const factory SubscriptionEvent.startTrial({
    required String hubId,
  }) = SubscriptionEventStartTrial;
  const factory SubscriptionEvent.upgradeTier({
    required String hubId,
    required SubscriptionTier tier,
  }) = SubscriptionEventUpgradeTier;
}
