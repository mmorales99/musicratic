namespace Musicratic.Playback.Application.DTOs;

public sealed record CollectiveVoteResultDto(
    Guid VoteSessionId,
    DateTime ExpiresAt,
    double RequiredApprovalPercentage);
