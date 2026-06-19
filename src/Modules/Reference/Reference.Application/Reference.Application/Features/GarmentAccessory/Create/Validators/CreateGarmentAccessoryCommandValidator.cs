namespace Reference.Application.Features.GarmentAccessory.Create.Validators;

public sealed class CreateGarmentAccessoryCommandValidator : AbstractValidator<CreateGarmentAccessoryCommand>
{
    public CreateGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення фурнітури не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор фурнітури має бути більшим за 0.");

            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва фурнітури є обов'язковою.")
                .MaximumLength(500)
                .WithMessage("Назва фурнітури не може перевищувати 500 символів.");

            RuleFor(x => x.Request.Price)
                .InclusiveBetween(0, 10_000)
                .WithMessage("Ціна фурнітури має бути від 0 до 10000.");

            RuleFor(x => x.Request.SupplierName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва постачальника фурнітури є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва постачальника фурнітури не може перевищувати 200 символів.");
        });
    }
}
