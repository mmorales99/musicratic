using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Application.DTOs;

public sealed record CollectiveVoteSessionDto(
    Guid Id,
    Guid TenantId,
    Guid QueueEntryId,
    Guid ProposerId,
    CollectiveVoteStatus Status,
    DateTime OpensAt,
    DateTime ExpiresAt,
    double RequiredApprovalPercentage);
