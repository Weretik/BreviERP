namespace Reference.Application.Features.Fabric.Update.Validators;

public sealed class UpdateFabricCommandValidator : AbstractValidator<UpdateFabricCommand>
{
    public UpdateFabricCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор тканини має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення тканини не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва тканини є обов'язковою.")
                .MaximumLength(500)
                .WithMessage("Назва тканини не може перевищувати 500 символів.");

            RuleFor(x => x.Request.Price)
                .InclusiveBetween(0, 10_000)
                .WithMessage("Ціна тканини має бути від 0 до 10000.");

            RuleFor(x => x.Request.ProviderName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва постачальника тканини є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва постачальника тканини не може перевищувати 200 символів.");
        });
    }
}
