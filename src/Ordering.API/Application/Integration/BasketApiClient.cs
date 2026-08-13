using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Exceptions;
using Ordering.API.Application.Contracts;

namespace Ordering.API.Application.Integration;

public class BasketApiClient(HttpClient httpClient, ILogger<BasketApiClient> logger) : IBasketApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<BasketDto?> GetBasketAsync(string basketId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"/api/basket/{Uri.EscapeDataString(basketId)}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<BasketDto>(content, JsonOptions);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
            || ex.InnerException is System.Net.Sockets.SocketException)
        {
            logger.LogError(ex, "No se pudo contactar con el microservicio Basket.API");
            throw new InternalServerException("No se pudo contactar con el microservicio de carritos. Intente nuevamente.");
        }
        catch (Exception ex) when (ex is not InternalServerException)
        {
            logger.LogError(ex, "Error al consultar el carrito {BasketId}", basketId);
            throw new InternalServerException("Ocurrió un error al consultar el carrito del cliente.");
        }
    }
}