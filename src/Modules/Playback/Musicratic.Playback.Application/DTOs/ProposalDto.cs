namespace Musicratic.Playback.Application.DTOs;

public sealed record ProposalDto(
    Guid QueueEntryId,
    Guid TrackId,
    string Title,
    string Artist,
    string Status,
    Guid? VoteSessionId,
    DateTime? VoteExpiresAt,
    double? RequiredApprovalPercentage);
