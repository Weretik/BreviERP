using BuildingBlocks.Application.Helpers;

namespace Reference.Application.Features.Supplier.Create.Validators;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Link)
            .MaximumLength(2048);

        RuleFor(x => x.Request.ContactPerson)
            .MaximumLength(200)
            .When(x => x.Request is not null && !string.IsNullOrWhiteSpace(x.Request.ContactPerson));

        RuleFor(x => x.Request.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberHelper.TryParse(phone, out _))
            .WithMessage("Phone number is invalid.");
    }
}
