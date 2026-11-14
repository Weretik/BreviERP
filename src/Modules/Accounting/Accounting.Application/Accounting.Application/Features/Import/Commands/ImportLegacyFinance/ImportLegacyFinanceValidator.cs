using Accounting.Application.Abstractions;
using Accounting.Application.Features.Import.Shared;

namespace Accounting.Application.Features.Import.Commands.ImportLegacyFinance;

public sealed class ImportLegacyFinanceValidator : AbstractValidator<ImportLegacyFinanceCommand>
{
    public ImportLegacyFinanceValidator() => RuleFor(x => x.Content).NotNull();
}
