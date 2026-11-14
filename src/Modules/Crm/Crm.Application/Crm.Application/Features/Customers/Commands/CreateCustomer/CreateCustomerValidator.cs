namespace Crm.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input != null, () =>
        {
            RuleFor(x => x.Input.Kind).Must(x => x is "organization" or "person");
            RuleFor(x => x.Input.DisplayName).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Input.LegalName).MaximumLength(500);
            RuleFor(x => x.Input.TaxIdentity).MaximumLength(80);
            RuleFor(x => x.Input.ExternalIdentities).NotNull().Must(x => x is { Count: <= 30 });
            RuleForEach(x => x.Input.ExternalIdentities).NotNull().ChildRules(v =>
            {
                v.RuleFor(x => x.Source).NotEmpty().MaximumLength(80);
                v.RuleFor(x => x.Type).NotEmpty().MaximumLength(80);
                v.RuleFor(x => x.ExternalId).NotEmpty().MaximumLength(200);
            });
        });
    }
}
