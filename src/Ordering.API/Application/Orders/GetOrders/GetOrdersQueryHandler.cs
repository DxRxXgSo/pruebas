using Ordering.API.Application.Contracts;

namespace Ordering.API.Application.Orders.GetOrders;

public record GetOrdersQuery : IQuery<GetOrdersResult>;
public record GetOrdersResult(List<Domain.Order> Orders);

public class GetOrdersQueryHandler(IOrderRepository repository)
    : IqueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        return new GetOrdersResult(orders);
    }
}
