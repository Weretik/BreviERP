using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Export.Queries.GetFinanceExport;

/// <summary>The JSON file and its name, <c>brevi-finance-YYYY-MM-DD.json</c> by the Kyiv day.</summary>
public sealed record FinanceExportFile(byte[] Content, string FileName);
