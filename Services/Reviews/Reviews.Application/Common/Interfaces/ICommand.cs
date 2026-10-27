namespace Reviews.Application.Common.Interfaces;

using MediatR;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
