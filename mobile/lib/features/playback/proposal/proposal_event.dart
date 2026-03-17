import 'package:freezed_annotation/freezed_annotation.dart';

part 'proposal_event.freezed.dart';

@freezed
class ProposalEvent with _$ProposalEvent {
  const factory ProposalEvent.searchTracks({
    required String query,
    String? provider,
  }) = ProposalEventSearchTracks;

  const factory ProposalEvent.selectTrack({
    required String trackId,
  }) = ProposalEventSelectTrack;

  const factory ProposalEvent.confirmProposal({
    required String hubId,
    required String trackId,
    required String providerId,
  }) = ProposalEventConfirmProposal;

  const factory ProposalEvent.cancelProposal() =
      ProposalEventCancelProposal;
}
