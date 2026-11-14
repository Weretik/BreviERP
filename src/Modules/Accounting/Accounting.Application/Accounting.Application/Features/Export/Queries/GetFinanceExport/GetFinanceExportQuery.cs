using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Export.Queries.GetFinanceExport;

/// <summary>
/// «Резервна копія»: every finance record in one JSON file — accounts, categories, counterparties, operations and
/// transfers (cancelled ones too), file details (not the files), the change log and the bank statements with their lines.
/// </summary>
public sealed record GetFinanceExportQuery : IQuery<Result<FinanceExportFile>>;
