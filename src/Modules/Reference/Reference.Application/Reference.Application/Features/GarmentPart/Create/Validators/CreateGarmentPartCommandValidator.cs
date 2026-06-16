using BuildingBlocks.Application.Helpers;

namespace Reference.Application.Features.GarmentPart.Create.Validators;

public sealed class CreateGarmentPartCommandValidator : AbstractValidator<CreateGarmentPartCommand>
{
    public CreateGarmentPartCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Request.SupplierId)
            .GreaterThan(0);

        RuleFor(x => x.Request.ContactPerson)
            .MaximumLength(200)
            .When(x => x.Request is not null && !string.IsNullOrWhiteSpace(x.Request.ContactPerson));

        RuleFor(x => x.Request.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberHelper.TryParse(phone, out _))
            .WithMessage("Phone number is invalid.");
    }
}
