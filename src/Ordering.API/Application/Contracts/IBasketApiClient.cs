using Ordering.API.Application.Integration;

namespace Ordering.API.Application.Contracts;

public interface IBasketApiClient
{
    Task<BasketDto?> GetBasketAsync(string basketId, CancellationToken cancellationToken = default);
}