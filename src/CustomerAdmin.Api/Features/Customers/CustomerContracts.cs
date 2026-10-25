using System.ComponentModel.DataAnnotations;
using CustomerAdmin.Api.Domain;
using CustomerAdmin.Api.Features.Contacts;

namespace CustomerAdmin.Api.Features.Customers;

/// <summary>Request body for creating a customer.</summary>
public record CustomerRequest
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    // Frontend should send empty input as null for email and phone.
    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Phone, StringLength(50, MinimumLength = 4)]
    public string? Phone { get; init; }

    [StringLength(200)]
    public string? Street { get; init; }

    [StringLength(20)]
    public string? PostalCode { get; init; }

    [StringLength(100)]
    public string? City { get; init; }

    [StringLength(2000)]
    public string? Note { get; init; }
}

/// <summary>Request body for updating a customer. Replaces all fields (PUT semantics).</summary>
public sealed record UpdateCustomerRequest : CustomerRequest
{
    /// <summary>False deactivates the customer, true reactivates it.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// The version the client has loaded. If the customer was changed in the meantime, the update fails with 409.
    /// </summary>
    [Range(1, uint.MaxValue)]
    public uint Version { get; init; }
}

/// <summary>A customer as shown in the list.</summary>
public sealed record CustomerSummaryResponse(
    int Id,
    string Name,
    string? City,
    string? Email,
    string? Phone,
    bool IsActive,
    int ContactCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A customer with all fields and its contacts, primary contact first.</summary>
public sealed record CustomerDetailResponse(
    int Id,
    string Name,
    string? Email,
    string? Phone,
    string? Street,
    string? PostalCode,
    string? City,
    string? Note,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    uint Version,
    IReadOnlyList<ContactResponse> Contacts)
{
    public static CustomerDetailResponse From(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Email,
        customer.Phone,
        customer.Street,
        customer.PostalCode,
        customer.City,
        customer.Note,
        customer.IsActive,
        customer.CreatedAt,
        customer.UpdatedAt,
        customer.Version,
        customer.Contacts
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(ContactResponse.From)
            .ToList());
}