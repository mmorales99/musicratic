import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../shared/models/hub.dart';
import '../models/list_models.dart';

part 'hub_detail_state.freezed.dart';

@freezed
class HubDetailState with _$HubDetailState {
  const factory HubDetailState.initial() = HubDetailStateInitial;
  const factory HubDetailState.loading() = HubDetailStateLoading;
  const factory HubDetailState.loaded({
    required Hub hub,
    required HubSettings settings,
    required List<HubList> lists,
  }) = HubDetailStateLoaded;
  const factory HubDetailState.acting({
    required Hub hub,
    required String action,
  }) = HubDetailStateActing;
  const factory HubDetailState.deleted() = HubDetailStateDeleted;
  const factory HubDetailState.error({required String message}) =
      HubDetailStateError;
}
