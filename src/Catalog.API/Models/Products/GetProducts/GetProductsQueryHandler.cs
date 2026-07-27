using Catalog.API.Common.Pagination;

namespace Catalog.API.Models.Products.GetProducts
{
    public record GetProductsQuery(string? Name, int PageIndex = 1, int PageSize = 10) : IQuery<GetProductsResult>;
    public record GetProductsResult(PaginatedResult<Product> Products);

    internal class GetProductsQueryHandler(IDocumentSession session, ILogger<GetProductsQueryHandler> logger)
        : IqueryHandler<GetProductsQuery, GetProductsResult>
    {
        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductsQueryHandler.Handle llamado con {@Query}", query);

            var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            IQueryable<Product> queryable = session.Query<Product>();

            if (!string.IsNullOrWhiteSpace(query.Name))
                queryable = queryable.Where(p => p.Name.ToLower().Contains(query.Name.ToLower()));

            var totalCount = await queryable.CountAsync(cancellationToken);

            var products = await queryable
                .OrderBy(p => p.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<Product>(pageIndex, pageSize, totalCount, products);
            return new GetProductsResult(result);
        }
    }
}
