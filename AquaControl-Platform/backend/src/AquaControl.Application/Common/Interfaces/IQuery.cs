using MediatR;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Common.Interfaces;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

