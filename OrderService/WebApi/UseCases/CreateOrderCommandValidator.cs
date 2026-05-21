using FluentValidation;

namespace OrderService.WebApi.UseCases;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.EmailClient).EmailAddress();
        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$");
    }
}