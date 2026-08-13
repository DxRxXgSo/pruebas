using Ordering.API.Application.Contracts;

namespace Ordering.API.Application.Orders.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IQuery<GetOrdersByCustomerResult>;
public record GetOrdersByCustomerResult(List<Domain.Order> Orders);

public class GetOrdersByCustomerQueryHandler(IOrderRepository repository)
    : IqueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetByCustomerAsync(query.CustomerId, cancellationToken);
        return new GetOrdersByCustomerResult(orders);
    }
}