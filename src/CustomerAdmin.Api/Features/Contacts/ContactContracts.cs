using System.ComponentModel.DataAnnotations;
using CustomerAdmin.Api.Domain;

namespace CustomerAdmin.Api.Features.Contacts;

/// <summary>Request body for creating a contact.</summary>
public record ContactRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    // Frontend should send empty input as null for email and phone.
    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Phone, StringLength(50, MinimumLength = 4)]
    public string? Phone { get; init; }

    [StringLength(100)]
    public string? Position { get; init; }

    [StringLength(2000)]
    public string? Note { get; init; }

    /// <summary>
    /// Makes this the primary contact. The previous primary contact of the customer loses the flag.
    /// </summary>
    public bool IsPrimary { get; init; }
}

/// <summary>Request body for updating a contact. Replaces all fields (PUT semantics).</summary>
public sealed record UpdateContactRequest : ContactRequest
{
    /// <summary>
    /// The version the client has loaded. If the contact was changed in the meantime, the update fails with 409.
    /// </summary>
    [Range(1, uint.MaxValue)]
    public uint Version { get; init; }
}

/// <summary>A contact person of a customer.</summary>
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
    DateTime CreatedAt,
    DateTime? UpdatedAt,
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
        contact.CreatedAt,
        contact.UpdatedAt,
        contact.Version);
}