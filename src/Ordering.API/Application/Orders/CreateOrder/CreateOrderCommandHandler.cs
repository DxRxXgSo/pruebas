using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Options;
using Ordering.API.Application.Contracts;
using Ordering.API.Application.Integration;
using Ordering.API.Domain;
using Ordering.API.Infrastructure.Configuration;

namespace Ordering.API.Application.Orders.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository repository,
    IBasketApiClient basketApiClient,
    ICatalogApiClient catalogApiClient,
    IOptions<OrderingSettings> settings,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            logger.LogInformation(
                "Solicitud idempotente con clave {Key}: se devuelve la orden existente {OrderId}",
                command.IdempotencyKey, existing.Id);

            return new CreateOrderResult(existing, Created: false);
        }

        var basket = await basketApiClient.GetBasketAsync(command.BasketId, cancellationToken)
            ?? throw new BadRequestException(
                $"El carrito \"{command.BasketId}\" no existe o no se encontró para el cliente.");

        if (basket.IsEmpty)
            throw new BadRequestException("El carrito está vacío. Agregue al menos un producto antes de generar la orden.");

        foreach (var item in basket.Items)
        {
            if (item.Quantity <= 0)
                throw new BadRequestException($"El producto \"{item.ProductName}\" tiene una cantidad inválida.");

            if (item.Price <= 0)
                throw new BadRequestException($"El producto \"{item.ProductName}\" tiene un precio inválido.");

            var product = await catalogApiClient.GetProductByNameAsync(item.ProductName, cancellationToken);
            if (product is null)
                throw new BadRequestException(
                    $"El producto \"{item.ProductName}\" no existe en el catálogo. La orden no puede generarse.");
        }

        var order = BuildOrder(command, basket, settings.Value.TaxRate);

        var saved = await repository.CreateAsync(order, cancellationToken);
        var created = saved.Id == order.Id;

        logger.LogInformation(
            "Orden {OrderId} creada para el cliente {CustomerId} por un total de {Total} (creada: {Created})",
            saved.Id, saved.CustomerId, saved.Total, created);

        return new CreateOrderResult(saved, created);
    }

    private static Domain.Order BuildOrder(CreateOrderCommand command, BasketDto basket, decimal taxRate)
    {
        var items = basket.Items.Select(item => new OrderItem
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.Price,
            LineTotal = Math.Round(item.Price * item.Quantity, 2)
        }).ToList();

        var subtotal = Math.Round(items.Sum(i => i.LineTotal), 2);
        var tax = Math.Round(subtotal * taxRate, 2);
        var total = Math.Round(subtotal + tax, 2);

        return new Domain.Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = command.CustomerId,
            IdempotencyKey = command.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = items,
            Subtotal = subtotal,
            Tax = tax,
            Total = total
        };
    }
}