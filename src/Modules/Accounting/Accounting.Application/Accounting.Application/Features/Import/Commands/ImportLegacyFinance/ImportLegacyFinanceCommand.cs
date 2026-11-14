using Accounting.Application.Abstractions;
using Accounting.Application.Features.Import.Shared;
using Accounting.Application.Features.Import.Shared.Report;

namespace Accounting.Application.Features.Import.Commands.ImportLegacyFinance;

/// <summary>
/// «Імпорт архіву Фінансів»: checks the export of the old Calculator (<c>DryRun</c>) or moves it into Brevi in one
/// transaction. A move with errors writes nothing and returns the report.
/// </summary>
public sealed record ImportLegacyFinanceCommand(Stream Content, bool DryRun)
    : ICommand<Result<LegacyFinanceImportReport>>;
