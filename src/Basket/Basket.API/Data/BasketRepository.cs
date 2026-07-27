using Basket.API.Exceptions;
using Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data
{
    public class BasketRepository(IDistributedCache cache) : IBasketRepository
    {
        public async Task<ShoppingCart> GetBasket(string userName,
            CancellationToken cancellationToken = default)
        {
            var cacheBasket = await cache.GetStringAsync(userName, cancellationToken);
            if (!string.IsNullOrEmpty(cacheBasket))
                return JsonSerializer.Deserialize<ShoppingCart>(cacheBasket)!;

            throw new BasketNotFoundException(userName);
        }

        public async Task<ShoppingCart> StoreBasket(ShoppingCart basket,
            CancellationToken cancellationToken = default)
        {
            await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }

        public async Task<bool> DeleteBasket(string userName,
            CancellationToken cancellationToken = default)
        {
            await cache.RemoveAsync(userName, cancellationToken);
            return true;
        }
    }
}
