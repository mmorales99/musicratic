import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/vote_models.dart';

part 'voting_state.freezed.dart';

@freezed
class VotingState with _$VotingState {
  const factory VotingState({
    @Default({}) Map<String, EntryVoteData> entries,
    SkipNotification? lastSkipNotification,
    String? error,
  }) = _VotingState;
}
