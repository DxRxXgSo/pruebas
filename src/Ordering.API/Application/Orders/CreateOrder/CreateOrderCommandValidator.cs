using FluentValidation;

namespace Ordering.API.Application.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId es requerido.");

        RuleFor(x => x.BasketId)
            .NotEmpty().WithMessage("BasketId es requerido.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("El header Idempotency-Key es requerido para evitar órdenes duplicadas.");
    }
}