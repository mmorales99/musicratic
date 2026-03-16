using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Queries.CheckProposalCooldown;

public sealed record CheckProposalCooldownQuery(
    Guid TenantId,
    Guid ProposerId) : IQuery<CooldownCheckResult>;
