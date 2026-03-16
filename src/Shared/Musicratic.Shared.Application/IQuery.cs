using MediatR;

namespace Musicratic.Shared.Application;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
