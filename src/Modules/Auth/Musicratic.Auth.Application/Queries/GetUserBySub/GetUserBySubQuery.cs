using Musicratic.Auth.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Queries.GetUserBySub;

public sealed record GetUserBySubQuery(string AuthentikSub) : IQuery<UserDto?>;
