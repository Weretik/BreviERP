namespace Reference.Application.Features.PricingParameter.Shared;

/// <summary>Editable fields of a pricing parameter card; the code is set once on creation.</summary>
public interface IPricingParameterFields
{
    string Group { get; }
    string Name { get; }
    decimal? Value { get; }
    string Unit { get; }
    bool IsCalculated { get; }
    string? Comment { get; }
    int SortOrder { get; }
}
