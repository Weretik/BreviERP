using Ardalis.Result;
using Crm.Application.Documents.Contracts;
using Mediator;

namespace Crm.Application.Documents.GetIssuerSettings;

public sealed record GetIssuerSettingsQuery : IQuery<Result<ActiveIssuerSettings>>;
