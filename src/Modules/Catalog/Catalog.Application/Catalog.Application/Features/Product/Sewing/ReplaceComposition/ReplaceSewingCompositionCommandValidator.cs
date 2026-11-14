namespace Catalog.Application.Features.Product.Sewing;

public sealed class ReplaceSewingCompositionCommandValidator : AbstractValidator<ReplaceSewingCompositionCommand>
{
    public ReplaceSewingCompositionCommandValidator()
    {
        RuleFor(x => x.ProductId).Must(SewingRules.ValidProductId).OverridePropertyName("productId")
            .WithMessage("Неправильний товар.");
        RuleFor(x => x.Request).NotNull().OverridePropertyName("request").WithMessage("Заповніть склад моделі.");
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.ExpectedVersion).GreaterThanOrEqualTo(0).OverridePropertyName("expectedVersion")
                .WithMessage("Версія не може бути від'ємною.");
            RuleFor(x => x.Request.ModelCode)
                .Must(code => code is null || code.Trim().Length <= SewingRules.MaxCodeLength)
                .OverridePropertyName("modelCode")
                .WithMessage($"Код моделі — не більше {SewingRules.MaxCodeLength} символів.");
            RuleFor(x => x.Request.Materials)
                .Must(lines => lines is not null && lines.Count <= SewingRules.MaxLines)
                .OverridePropertyName("materials")
                .WithMessage($"Вкажіть не більше {SewingRules.MaxLines} рядків матеріалів.");
            RuleForEach(x => x.Request.Materials)
                .NotNull().WithMessage("Рядок матеріалу порожній.")
                .SetValidator(new SewingMaterialInputValidator())
                .OverridePropertyName("materials")
                .When(x => x.Request.Materials is { Count: <= SewingRules.MaxLines });
            RuleFor(x => x.Request.Operations)
                .Must(lines => lines is not null && lines.Count <= SewingRules.MaxLines)
                .OverridePropertyName("operations")
                .WithMessage($"Вкажіть не більше {SewingRules.MaxLines} рядків операцій.");
            RuleForEach(x => x.Request.Operations)
                .NotNull().WithMessage("Рядок операції порожній.")
                .SetValidator(new SewingOperationInputValidator())
                .OverridePropertyName("operations")
                .When(x => x.Request.Operations is { Count: <= SewingRules.MaxLines });
        });
    }
}
