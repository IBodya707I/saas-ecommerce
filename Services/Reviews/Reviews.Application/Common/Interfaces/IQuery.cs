namespace Reviews.Application.Common.Interfaces;

using MediatR;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
