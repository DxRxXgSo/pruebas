using Basket.API.Basket.StoreBasket;
using Basket.API.Data;
using Basket.API.Models;

namespace Basket.API.Basket.GetBasket
{
    public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
    public record GetBasketResult(ShoppingCart Cart);
    public class GetBasketCommandHandler(IBasketRepository repository)
        : IqueryHandler<GetBasketQuery, GetBasketResult>
    {
        public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
        {
            var sanitizedUserName = query.UserName?.Trim().Replace(" ", "_") ?? "anonymous";
            var basket = await repository.GetBasket(sanitizedUserName);
            return new GetBasketResult(basket);
        }
    }
}
