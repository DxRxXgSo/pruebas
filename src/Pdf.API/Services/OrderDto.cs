namespace Pdf.API.Services;

public record OrderItemDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderDto(
    string Id,
    string CustomerId,
    DateTime CreatedAt,
    string Status,
    List<OrderItemDto> Items,
    decimal Subtotal,
    decimal Tax,
    decimal Total);