namespace Ordering.API.Domain;

public class OrderItem
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}