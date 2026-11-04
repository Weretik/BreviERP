using Crm.Application.Quotes.Access;

namespace Crm.Application.Quotes.Introductions.Templates.DeleteTemplate;

/// <summary>R-19: the author, Director or Admin deletes a template.</summary>
public sealed record DeleteQuoteIntroTemplateCommand(QuoteActor Actor, long Id) : ICommand<Result>;
