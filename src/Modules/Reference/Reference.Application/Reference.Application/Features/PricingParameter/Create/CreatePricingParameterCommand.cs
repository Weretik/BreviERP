using Reference.Application.Features.PricingParameter.Create.DTOs;

namespace Reference.Application.Features.PricingParameter.Create;

public sealed record CreatePricingParameterCommand(CreatePricingParameterCommandRequest Request) : ICommand<Result<int>>;
