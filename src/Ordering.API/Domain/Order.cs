using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ordering.API.Domain;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = default!;

    public string CustomerId { get; set; } = default!;

    [BsonIgnoreIfNull]
    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }
}