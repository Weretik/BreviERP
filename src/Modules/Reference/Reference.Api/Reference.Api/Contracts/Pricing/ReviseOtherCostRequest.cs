using System.Text.Json.Serialization;

namespace Reference.Api.Contracts.Pricing;

public sealed record ReviseOtherCostRequest(
    [property: JsonRequired] long ExpectedVersionId,
    [property: JsonRequired] decimal Value,
    [property: JsonRequired] Guid RequestId);
