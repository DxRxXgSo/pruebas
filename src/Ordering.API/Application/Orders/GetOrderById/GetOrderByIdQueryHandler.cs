using BuildingBlocks.Exceptions;
using Ordering.API.Application.Contracts;

namespace Ordering.API.Application.Orders.GetOrderById;

public record GetOrderByIdQuery(string OrderId) : IQuery<GetOrderByIdResult>;
public record GetOrderByIdResult(Domain.Order Order);

public class GetOrderByIdQueryHandler(IOrderRepository repository)
    : IqueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(query.OrderId, cancellationToken)
            ?? throw new NotFoundException($"La orden \"{query.OrderId}\" no fue encontrada.");

        return new GetOrderByIdResult(order);
    }
}