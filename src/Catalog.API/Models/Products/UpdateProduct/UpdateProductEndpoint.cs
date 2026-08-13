namespace Catalog.API.Models.Products.UpdateProduct
{
    public record UpdateProductRequest(string Name, string Description, List<string> Category, string ImagesFiles, decimal Price, string ImageUrl = default, int Stock = 10);
    public record UpdateProductResponse(bool IsSuccess);

    public class UpdateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/products/{name}", async (string name, UpdateProductRequest request, ISender sender) =>
            {
                var command = new UpdateProductCommand(name, request.Name, request.Description, request.Category, request.ImagesFiles, request.Price, request.ImageUrl, request.Stock);
                var result = await sender.Send(command);
                return Results.Ok(new UpdateProductResponse(result.IsSuccess));
            })
                .WithName("ActualizarProducto")
                .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Actualiza un producto por nombre")
                .WithDescription("Busca el producto por su nombre actual (en la ruta) y actualiza sus datos con el cuerpo de la peticion.");
        }
    }
}
