using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Exceptions;

namespace Pdf.API.Services;

public class OrderingApiClient(HttpClient httpClient, ILogger<OrderingApiClient> logger) : IOrderingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<OrderDto?> GetOrderByIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"/api/orders/{Uri.EscapeDataString(orderId)}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new InternalServerException("No fue posible consultar la orden de compra.");

            return await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
        {
            logger.LogError(ex, "No se pudo contactar con el microservicio Ordering.API");
            throw new InternalServerException("No se pudo contactar con el microservicio de órdenes.");
        }
        catch (Exception ex) when (ex is not InternalServerException)
        {
            logger.LogError(ex, "Error al consultar la orden {OrderId} en Ordering.API", orderId);
            throw new InternalServerException("Ocurrió un error al consultar la orden de compra.");
        }
    }

    public async Task<List<OrderDto>> GetOrdersByCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                $"/api/orders/customer/{Uri.EscapeDataString(customerId)}", cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InternalServerException("No fue posible consultar las órdenes del cliente.");

            return await response.Content.ReadFromJsonAsync<List<OrderDto>>(JsonOptions, cancellationToken)
                ?? [];
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
        {
            logger.LogError(ex, "No se pudo contactar con el microservicio Ordering.API");
            throw new InternalServerException("No se pudo contactar con el microservicio de órdenes.");
        }
        catch (Exception ex) when (ex is not InternalServerException)
        {
            logger.LogError(ex, "Error al consultar las órdenes del cliente {CustomerId}", customerId);
            throw new InternalServerException("Ocurrió un error al consultar las órdenes del cliente.");
        }
    }
}