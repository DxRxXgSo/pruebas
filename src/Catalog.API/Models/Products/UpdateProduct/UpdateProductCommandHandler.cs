using Catalog.API.Exceptions;

namespace Catalog.API.Models.Products.UpdateProduct
{
    public record UpdateProductCommand(
        string CurrentName,
        string Name,
        string Description,
        List<string> Category,
        string ImagesFiles,
        decimal Price,
        string ImageUrl = default,
        int Stock = 10) : ICommand<UpdateProductResult>;

    public record UpdateProductResult(bool IsSuccess);

    internal class UpdateProductCommandHandler(IDocumentSession session)
        : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            var product = await session.Query<Product>()
                .FirstOrDefaultAsync(p => p.Name == command.CurrentName, cancellationToken);

            if (product is null)
                throw new ProductNotFoundException(command.CurrentName);

            product.Name = command.Name;
            product.Descripcion = command.Description;
            product.Category = command.Category;
            product.ImageFiles = command.ImagesFiles;
            product.ImageUrl = command.ImageUrl;
            product.Price = command.Price;
            product.Stock = command.Stock;

            session.Update(product);
            await session.SaveChangesAsync(cancellationToken);

            return new UpdateProductResult(true);
        }
    }
}
