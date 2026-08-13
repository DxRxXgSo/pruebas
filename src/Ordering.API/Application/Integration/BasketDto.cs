namespace Ordering.API.Application.Integration;

public record BasketDto(string? UserName, List<BasketItemDto> Items)
{
    public bool IsEmpty => Items == null || Items.Count == 0;
}

public record BasketItemDto(
    int Quantity,
    string Color,
    decimal Price,
    string ProductId,
    string ProductName,
    string ImageFile,
    string ImageUrl);