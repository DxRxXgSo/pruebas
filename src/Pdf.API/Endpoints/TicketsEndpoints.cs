using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Options;
using Pdf.API.Services;

namespace Pdf.API.Endpoints;

public class TicketEndpoints
{
    public static void MapTicketEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .WithOpenApi();

        group.MapGet("/{orderId}", async (
            string orderId,
            IOrderingApiClient orderingApiClient,
            IOptions<PdfSettings> settings,
            CancellationToken cancellationToken) =>
        {
            var order = await orderingApiClient.GetOrderByIdAsync(orderId, cancellationToken)
                ?? throw new NotFoundException($"La orden \"{orderId}\" no existe.");

            var pdf = PdfTicketGenerator.GenerateOrderTicket(order, settings.Value.StoreName, settings.Value.TaxRate);

            return Results.File(
                pdf,
                "application/pdf",
                fileDownloadName: $"ticket-{order.Id}.pdf");
        })
        .WithName("GetOrderTicket")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Ticket PDF de una orden")
        .WithDescription("Genera un ticket PDF (estilo recibo de tienda) con los productos, subtotal, impuestos y total de la orden indicada.");

        group.MapGet("/customer/{customerId}", async (
            string customerId,
            IOrderingApiClient orderingApiClient,
            IOptions<PdfSettings> settings,
            CancellationToken cancellationToken) =>
        {
            var orders = await orderingApiClient.GetOrdersByCustomerAsync(customerId, cancellationToken);

            if (orders.Count == 0)
                throw new NotFoundException($"El cliente \"{customerId}\" no tiene órdenes registradas.");

            var pdf = PdfTicketGenerator.GenerateCustomerSummaryTicket(orders, customerId, settings.Value.StoreName);

            return Results.File(
                pdf,
                "application/pdf",
                fileDownloadName: $"tickets-{customerId}.pdf");
        })
        .WithName("GetCustomerTickets")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Resumen PDF de compras de un cliente")
        .WithDescription("Genera un PDF con todas las órdenes del cliente y el total general de sus compras.");

        return;
    }
}

public record PdfSettings
{
    public string OrderingApiBaseUrl { get; set; } = string.Empty;
    public string StoreName { get; set; } = "E-Shop";
    public decimal TaxRate { get; set; } = 0.16m;
}