using BuildingBlocks.Application.Helpers;

namespace Reference.Application.Features.Supplier.Create.Validators;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення постачальника не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор постачальника має бути більшим за 0.");

            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва постачальника є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва постачальника не може перевищувати 200 символів.");

            RuleFor(x => x.Request.Link)
                .MaximumLength(2048)
                .WithMessage("Посилання постачальника не може перевищувати 2048 символів.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Link));

            RuleFor(x => x.Request.ContactPerson)
                .MaximumLength(200)
                .WithMessage("Ім'я контактної особи не може перевищувати 200 символів.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.ContactPerson));

            RuleFor(x => x.Request.PhoneNumber)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberHelper.TryParse(phone, out _))
                .WithMessage("Номер телефону має бути у коректному форматі.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.PhoneNumber));

            RuleFor(x => x.Request.Notes)
                .MaximumLength(500)
                .WithMessage("Примітки не можуть перевищувати 500 символів.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Notes));
        });
    }
}
