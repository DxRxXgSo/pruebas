using Catalog.API.Exceptions;

namespace Catalog.API.Models.Products.ReduceStock
{
    public record ReduceStockCommand(string Name, int Quantity) : ICommand<ReduceStockResult>;
    public record ReduceStockResult(bool IsSuccess, int Stock);

    internal class ReduceStockCommandHandler(IDocumentSession session)
        : ICommandHandler<ReduceStockCommand, ReduceStockResult>
    {
        public async Task<ReduceStockResult> Handle(ReduceStockCommand command, CancellationToken cancellationToken)
        {
            var product = await session.Query<Product>()
                .FirstOrDefaultAsync(p => p.Name == command.Name, cancellationToken);

            if (product is null)
                throw new ProductNotFoundException(command.Name);

            if (command.Quantity <= 0)
                throw new FluentValidation.ValidationException("La cantidad a descontar debe ser mayor que cero.");

            if (product.Stock < command.Quantity)
                throw new FluentValidation.ValidationException(
                    $"No hay stock suficiente de \"{product.Name}\": la cantidad solicitada es {command.Quantity} " +
                    $"y solo hay {product.Stock} disponible(s).");

            product.Stock -= command.Quantity;

            session.Update(product);
            await session.SaveChangesAsync(cancellationToken);

            return new ReduceStockResult(true, product.Stock);
        }
    }
}