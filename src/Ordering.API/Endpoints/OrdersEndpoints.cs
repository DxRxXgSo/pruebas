using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Application.Orders.CreateOrder;
using Ordering.API.Application.Orders.GetOrderById;
using Ordering.API.Application.Orders.GetOrdersByCustomer;
using Ordering.API.Application.Orders.UpdateOrderStatus;
using Ordering.API.Domain;

namespace Ordering.API.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapPost("/", async (CreateOrderRequest request, HttpRequest httpRequest, ISender sender) =>
        {
            var idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString();
            var result = await sender.Send(new CreateOrderCommand(
                request.CustomerId,
                request.BasketId,
                idempotencyKey));

            return result.Created
                ? Results.Created($"/api/orders/{result.Order.Id}", result.Order)
                : Results.Ok(result.Order);
        })
        .WithName("CreateOrder")
        .Produces<Domain.Order>(StatusCodes.Status201Created)
        .Produces<Domain.Order>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Generar orden de compra")
        .WithDescription(
            "Crea una orden de compra a partir del carrito del cliente. " +
            "Requiere el header Idempotency-Key: si se reenvía la misma solicitud con la misma clave, " +
            "no se crea una segunda orden y se devuelve la orden previamente generada.");

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id));
            return Results.Ok(result.Order);
        })
        .WithName("GetOrderById")
        .Produces<Domain.Order>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consultar orden por identificador")
        .WithDescription("Recupera una orden de compra por su identificador único.");

        group.MapGet("/customer/{customerId}", async (string customerId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));
            return Results.Ok(result.Orders);
        })
        .WithName("GetOrdersByCustomer")
        .Produces<List<Domain.Order>>(StatusCodes.Status200OK)
        .WithSummary("Órdenes por cliente")
        .WithDescription("Lista todas las órdenes de compra de un cliente.");

        group.MapPatch("/{id}/status", async (string id, [FromBody] UpdateOrderStatusRequest request, ISender sender) =>
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
                throw new BadRequestException(
                    $"Estado inválido: \"{request.Status}\". Valores permitidos: Pending, Confirmed, Cancelled.");

            var result = await sender.Send(new UpdateOrderStatusCommand(id, status));
            return Results.Ok(result.Order);
        })
        .WithName("UpdateOrderStatus")
        .Produces<Domain.Order>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Actualizar estado de la orden")
        .WithDescription(
            "Cambia el estado de la orden validando las transiciones permitidas: " +
            "Pending -> Confirmed y Pending -> Cancelled. Una orden Cancelled no puede regresar a Confirmed.");

        return app;
    }
}

public record CreateOrderRequest(string CustomerId, string BasketId);

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}