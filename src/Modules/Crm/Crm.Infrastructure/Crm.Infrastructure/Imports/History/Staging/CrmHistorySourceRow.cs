using System.Text.Json;
using Crm.Application.Imports.History.Contracts;

namespace Crm.Infrastructure.Imports.History.Staging;

internal sealed record CrmHistorySourceRow(CrmHistoryRow Metadata, JsonElement Source);
