using Ordering.API.Application.Integration;

namespace Ordering.API.Application.Contracts;

public interface ICatalogApiClient
{
    Task<ProductDto?> GetProductByNameAsync(string name, CancellationToken cancellationToken = default);
}