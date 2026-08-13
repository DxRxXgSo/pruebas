using BuildingBlocks.Exceptions;
using Ordering.API.Application.Contracts;
using Ordering.API.Domain;

namespace Ordering.API.Application.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(string OrderId, OrderStatus Status) : ICommand<UpdateOrderStatusResult>;
public record UpdateOrderStatusResult(Domain.Order Order);

public class UpdateOrderStatusCommandHandler(IOrderRepository repository)
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"La orden \"{command.OrderId}\" no fue encontrada.");

        if (!IsValidTransition(order.Status, command.Status))
            throw new ConflictException(
                $"Transición de estado inválida: {order.Status} -> {command.Status}. " +
                "Solo se permiten Pending -> Confirmed y Pending -> Cancelled.");

        await repository.UpdateStatusAsync(order.Id, command.Status, cancellationToken);

        order.Status = command.Status;
        return new UpdateOrderStatusResult(order);
    }

    private static bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        return current == OrderStatus.Pending
            && next is OrderStatus.Confirmed or OrderStatus.Cancelled;
    }
}