using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Application.DTOs;

public sealed record VoteDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid QueueEntryId,
    VoteValue Value,
    DateTime CastAt);
