using Ordering.API.Domain;

namespace Ordering.API.Application.Contracts;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(string id, OrderStatus status, CancellationToken cancellationToken = default);
}