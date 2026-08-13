namespace Ordering.API.Infrastructure.Configuration;

public class OrderingSettings
{
    public string MongoDbConnectionString { get; set; } = string.Empty;
    public string MongoDbDatabaseName { get; set; } = "OrderingDb";
    public string BasketApiBaseUrl { get; set; } = string.Empty;
    public string CatalogApiBaseUrl { get; set; } = string.Empty;
    public decimal TaxRate { get; set; } = 0.16m;
}