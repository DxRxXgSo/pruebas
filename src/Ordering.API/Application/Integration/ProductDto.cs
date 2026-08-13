namespace Ordering.API.Application.Integration;

public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    int? Stock);

public record PaginatedProductsResponse(
    int PageIndex,
    int PageSize,
    long Count,
    List<ProductDto> Data);