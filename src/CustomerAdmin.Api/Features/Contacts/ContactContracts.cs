using CustomerAdmin.Api.Domain;

namespace CustomerAdmin.Api.Features.Contacts;

public sealed record ContactResponse(
    int Id,
    int CustomerId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position,
    string? Note,
    bool IsPrimary,
    uint Version)
{
    public static ContactResponse From(Contact contact) => new(
        contact.Id,
        contact.CustomerId,
        contact.FirstName,
        contact.LastName,
        contact.Email,
        contact.Phone,
        contact.Position,
        contact.Note,
        contact.IsPrimary,
        contact.Version);
}