using Basket.API.Models;

namespace Basket.API.Basket.GetBasket
{
    public class GetBasketEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/basket/{userName}", async (string userName, ISender sender) =>
            {
                var result = await sender.Send(new GetBasketQuery(userName));
                return Results.Ok(result.Cart);
            })
                .WithName("GetBasket")
                .Produces<ShoppingCart>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Get Basket By UserName")
                .WithDescription("Devuelve el carrito plano, sin wrappers de CQRS.");
        }
    }
}
