using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.DeleteQuoteIntroTemplate;

/// <summary>R-19: the author, Director or Admin deletes a template.</summary>
public sealed record DeleteQuoteIntroTemplateCommand(QuoteActor Actor, long Id) : ICommand<Result>;
