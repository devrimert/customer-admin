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
    /// Deactivating is the normal, reversible way to hide a customer.
    /// Deleting is permanent and also deletes all contacts, for example on a GDPR erasure request.
    /// A customer is a company and is usually not personal data; its contacts are.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public List<Contact> Contacts { get; set; } = [];
}