using MediatR;

namespace AccountingERP.Application.Interfaces;

/// <summary>
/// Marker interface cho Queries (CQRS)
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker interface cho Commands (CQRS)
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}
