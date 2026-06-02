using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class Supplier : BaseEntity<SupplierId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public string? Link { get; private set; }
    #endregion

    #region Constructors
    private Supplier() { }

    private Supplier(SupplierId id, string name, string? link)
    {
        SetId(id);
        SetName(name);
        SetLink(link);
    }
    #endregion

    #region Factories
    public static Supplier Create(SupplierId id, string name, string? link = null) => new(id, name, link);

    public void Update(string name, string? link = null)
    {
        SetName(name);
        SetLink(link);
    }
    #endregion

    #region Setters/Validation
    private void SetId(SupplierId id)
    {
        if (id.Value == default)
            throw new DomainException(SupplierErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(SupplierErrors.NameIsRequired());

        Name = name.Trim();
    }

    private void SetLink(string? link)
    {
        Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim();
    }
    #endregion
}
