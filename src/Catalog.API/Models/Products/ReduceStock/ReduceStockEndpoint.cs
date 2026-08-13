using Carter;
using MediatR;

namespace Catalog.API.Models.Products.ReduceStock
{
    public class ReduceStockRequest
    {
        public int Quantity { get; set; }
    }
    public record ReduceStockResponse(bool IsSuccess, int Stock);

    public class ReduceStockEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/api/products/{name}/stock", async (string name, ReduceStockRequest request, ISender sender) =>
            {
                var command = new ReduceStockCommand(name, request.Quantity);
                var result = await sender.Send(command);
                return Results.Ok(new ReduceStockResponse(result.IsSuccess, result.Stock));
            })
                .WithName("ReducirStockProducto")
                .Produces<ReduceStockResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Reduce el stock de un producto")
                .WithDescription("Descuenta la cantidad indicada del stock del producto al realizar una compra. " +
                    "Si el stock es insuficiente responde 400.");
        }
    }
}