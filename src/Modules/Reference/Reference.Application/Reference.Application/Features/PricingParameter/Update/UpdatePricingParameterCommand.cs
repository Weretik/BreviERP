using Reference.Application.Features.PricingParameter.Update.DTOs;

namespace Reference.Application.Features.PricingParameter.Update;

public sealed record UpdatePricingParameterCommand(int Id, UpdatePricingParameterCommandRequest Request) : ICommand<Result>;
