using BuildingBlocks.Exceptions;
using MongoDB.Driver;
using Ordering.API.Application.Contracts;
using Ordering.API.Domain;

namespace Ordering.API.Infrastructure.Persistence;

public class MongoDbOrderRepository(
    OrderingDbContext dbContext,
    ILogger<MongoDbOrderRepository> logger) : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders = dbContext.Orders;

    public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar la orden {OrderId}", id);
            throw new InternalServerException("Ocurrió un error al consultar la orden.");
        }
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _orders.Find(o => o.IdempotencyKey == idempotencyKey).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar la clave de idempotencia {Key}", idempotencyKey);
            throw new InternalServerException("Ocurrió un error al verificar la solicitud.");
        }
    }

    public async Task<List<Order>> GetByCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _orders
                .Find(o => o.CustomerId == customerId)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar las órdenes del cliente {CustomerId}", customerId);
            throw new InternalServerException("Ocurrió un error al consultar las órdenes del cliente.");
        }
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _orders.InsertOneAsync(order, cancellationToken: cancellationToken);
            return order;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(
                "Clave de idempotencia duplicada {Key} para el cliente {CustomerId}. Se devuelve la orden existente.",
                order.IdempotencyKey, order.CustomerId);

            return await GetByIdempotencyKeyAsync(order.IdempotencyKey!, cancellationToken)
                ?? throw new InternalServerException("No se pudo recuperar la orden existente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al persistir la orden en MongoDB Atlas");
            throw new InternalServerException("No se pudo guardar la orden. Intente nuevamente.");
        }
    }

    public async Task<bool> UpdateStatusAsync(string id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orders.UpdateOneAsync(
                o => o.Id == id,
                Builders<Order>.Update.Set(o => o.Status, status),
                cancellationToken: cancellationToken);

            return result.ModifiedCount > 0 || result.MatchedCount > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar el estado de la orden {OrderId}", id);
            throw new InternalServerException("Ocurrió un error al actualizar el estado de la orden.");
        }
    }
}