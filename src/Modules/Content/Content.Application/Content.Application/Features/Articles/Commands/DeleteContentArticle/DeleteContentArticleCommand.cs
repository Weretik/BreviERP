using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.DeleteContentArticle;

/// <summary>Removes an article with its texts (CNT-004 R-03); its photos stay in the «Фото» library.</summary>
public sealed record DeleteContentArticleCommand(int Id) : ICommand<Result>;
