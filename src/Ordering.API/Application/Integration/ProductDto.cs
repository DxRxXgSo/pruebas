namespace Ordering.API.Application.Integration;

public record ProductDto(
    string Id,
    string Name,
    decimal Price);

public record PaginatedProductsResponse(
    int PageIndex,
    int PageSize,
    long Count,
    List<ProductDto> Data);