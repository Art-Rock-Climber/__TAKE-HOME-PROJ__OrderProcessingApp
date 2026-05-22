using FluentValidation;

namespace PaymentService.WebApi.UseCases;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Price).GreaterThan(0);
    }
}