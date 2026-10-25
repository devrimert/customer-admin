namespace CustomerAdmin.Api.Domain;

/// <summary>
/// A contact person at a customer. Contains personal data (GDPR), so contacts can be deleted
/// individually and are deleted together with their customer.
/// </summary>
public class Contact : Entity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string? Note { get; set; }

    /// <summary>
    /// The main contact person of the customer. At most one per customer, enforced by the database.
    /// </summary>
    public bool IsPrimary { get; set; }
}