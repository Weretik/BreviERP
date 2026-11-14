using System.Text.Json.Serialization;
using Content.Application.Features.SeoOverview.Shared;

namespace Content.Application.Features.SeoOverview.Queries.GetSeoOverview;

public sealed record SeoAreaSummary(SeoArea Area, int OnSite, int WithProblems);
