
namespace Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;

public sealed record UpdatePricingParameterCommand(int Id, UpdatePricingParameterCommandRequest Request) : ICommand<Result>;
