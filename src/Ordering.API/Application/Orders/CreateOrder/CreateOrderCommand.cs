namespace Ordering.API.Application.Orders.CreateOrder;

public record CreateOrderCommand(
    string CustomerId,
    string BasketId,
    string IdempotencyKey) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Domain.Order Order, bool Created);