import 'package:freezed_annotation/freezed_annotation.dart';

import '../models/search_models.dart';

part 'proposal_state.freezed.dart';

@freezed
class ProposalState with _$ProposalState {
  const factory ProposalState.initial() = ProposalInitial;

  const factory ProposalState.searching({
    required String query,
    String? provider,
  }) = ProposalSearching;

  const factory ProposalState.results({
    required String query,
    String? provider,
    required List<TrackSearchResult> tracks,
  }) = ProposalResults;

  const factory ProposalState.selected({
    required TrackSearchResult track,
    required int coinCost,
    required int currentBalance,
  }) = ProposalSelected;

  const factory ProposalState.proposing({
    required TrackSearchResult track,
  }) = ProposalProposing;

  const factory ProposalState.success({
    required String message,
  }) = ProposalSuccess;

  const factory ProposalState.error({
    required String message,
    ProposalState? previousState,
  }) = ProposalError;
}
