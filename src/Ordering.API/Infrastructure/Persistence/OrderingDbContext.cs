using MongoDB.Bson;
using MongoDB.Driver;

namespace Ordering.API.Infrastructure.Persistence;

public class OrderingDbContext
{
    private readonly IMongoDatabase? _database;

    public OrderingDbContext(string connectionString, string databaseName)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.SslSettings = new SslSettings
            {
                CheckCertificateRevocation = false,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
            var client = new MongoClient(settings);
            _database = client.GetDatabase(databaseName);
        }
    }

    public IMongoCollection<Domain.Order> Orders =>
        _database?.GetCollection<Domain.Order>("orders")
        ?? throw new InvalidOperationException(
            "MongoDB no está configurado: falta la cadena de conexión (Ordering__MongoDbConnectionString).");

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var orders = Orders;

        var idempotencyIndex = new CreateIndexModel<Domain.Order>(
            Builders<Domain.Order>.IndexKeys.Ascending(o => o.IdempotencyKey),
            new CreateIndexOptions<Domain.Order>
            {
                Unique = true,
                PartialFilterExpression = new BsonDocument("IdempotencyKey", new BsonDocument("$exists", true))
            });

        var customerIndex = new CreateIndexModel<Domain.Order>(
            Builders<Domain.Order>.IndexKeys.Ascending(o => o.CustomerId));

        await orders.Indexes.CreateManyAsync([idempotencyIndex, customerIndex], cancellationToken);
    }
}