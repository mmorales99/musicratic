import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../shared/models/hub.dart';

part 'hub_search_state.freezed.dart';

@freezed
class HubSearchState with _$HubSearchState {
  const factory HubSearchState({
    @Default('') String query,
    String? businessType,
    String? visibility,
    @Default([]) List<Hub> hubs,
    @Default(1) int currentPage,
    @Default(false) bool hasMore,
    @Default(false) bool isLoading,
    @Default(false) bool isLoadingMore,
    String? errorMessage,
  }) = _HubSearchState;
}
