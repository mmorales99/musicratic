import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/search_models.dart';

part 'search_state.freezed.dart';

@freezed
class SearchState with _$SearchState {
  const factory SearchState({
    @Default('') String query,
    @Default([]) List<TrackSearchResult> results,
    String? provider,
    @Default(false) bool isLoading,
    @Default(false) bool isProposing,
    String? errorMessage,
    String? proposalSuccess,
  }) = _SearchState;
}
