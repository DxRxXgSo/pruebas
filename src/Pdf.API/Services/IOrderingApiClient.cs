namespace Pdf.API.Services;

public interface IOrderingApiClient
{
    Task<OrderDto?> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetOrdersByCustomerAsync(string customerId, CancellationToken cancellationToken = default);
}