namespace Crm.Application.Customers.SaveCustomerContact;

public sealed class SaveCustomerContactCommandValidator : AbstractValidator<SaveCustomerContactCommand>
{
    public SaveCustomerContactCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input != null, () =>
        {
            RuleFor(x => x.Input.Name).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Input.Position).MaximumLength(200);
            RuleFor(x => x.Input.Channels).NotNull().Must(x => x is { Count: <= 10 });
            RuleForEach(x => x.Input.Channels).NotNull();
        });
    }
}
