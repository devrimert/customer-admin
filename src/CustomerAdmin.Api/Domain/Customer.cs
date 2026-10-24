namespace CustomerAdmin.Api.Domain;

public class Customer : Entity
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Note { get; set; }
    /// <summary>
    /// Customers are deactivated instead of deleted, so their contacts stay intact.
    /// A customer is a company and is usually not personal data under the GDPR.
    /// Contact persons are personal data and are deleted individually (GDPR Art. 17, right to erasure).
    /// </summary>
    public bool IsActive { get; set; } = true;

    public List<Contact> Contacts { get; set; } = [];
}