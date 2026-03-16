using Musicratic.Auth.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;
