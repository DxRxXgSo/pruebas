using System.Text.Json;
using BuildingBlocks.Exceptions;
using Ordering.API.Application.Contracts;

namespace Ordering.API.Application.Integration;

public class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger) : ICatalogApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProductDto?> GetProductByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/products?name={Uri.EscapeDataString(name)}&pageIndex=1&pageSize=10";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = JsonSerializer.Deserialize<PaginatedProductsResponse>(content, JsonOptions);

            return page?.Data?.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex) when (ex is not InternalServerException)
        {
            logger.LogError(ex, "Error al validar el producto {ProductName} en el catálogo", name);
            throw new InternalServerException("Ocurrió un error al validar los productos del catálogo.");
        }
    }

    public async Task DecrementStockAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        var url = $"/api/products/{Uri.EscapeDataString(productName)}/stock";
        var body = JsonContent.Create(new { quantity });

        using var response = await httpClient.PatchAsync(url, body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}