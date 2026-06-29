using Reference.Domain.Errors;

namespace Reference.Domain.Suppliers.Errors;

public static class SupplierErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.Supplier.Id.Required", "������������ ������������� � ����'�������.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.Supplier.Name.Required", "����� ������������� � ����'�������.");

    public static ReferenceDomainError NotesLengthInvalid(int length) =>
        new("Reference.Supplier.Notes.LengthInvalid",
            $"������� ������� ������������� �� ���� ������������ 500 �������. ������� �������: {length}");
}
