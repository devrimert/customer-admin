using CustomerAdmin.Api.Common;
using CustomerAdmin.Api.Data;
using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerAdmin.Api.Features.Contacts;

public sealed class ContactService(CustomerAdminDbContext db)
{
    /// <returns>The contacts of the customer, primary contact first, or null if the customer does not exist.</returns>
    public async Task<IReadOnlyList<ContactResponse>?> GetListAsync(
        int customerId,
        string? search,
        CancellationToken cancellationToken)
    {
        var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
        {
            return null;
        }

        var query = db.Contacts
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = SearchPatterns.Contains(search);
            query = query.Where(c => EF.Functions.ILike(
                c.FirstName + " " + c.LastName, pattern, SearchPatterns.EscapeCharacter));
        }

        var contacts = await query
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);

        return contacts.Select(ContactResponse.From).ToList();
    }

    public async Task<ContactResponse?> GetByIdAsync(
        int customerId,
        int contactId,
        CancellationToken cancellationToken)
    {
        var contact = await db.Contacts
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == contactId && c.CustomerId == customerId, cancellationToken);

        return contact is null ? null : ContactResponse.From(contact);
    }

    /// <summary>
    /// Returns the current values of a contact as an update request. Used as the target for JSON Patch.
    /// </summary>
    public async Task<UpdateContactRequest?> GetUpdateRequestAsync(
        int customerId,
        int contactId,
        CancellationToken cancellationToken) =>
        await db.Contacts
            .AsNoTracking()
            .Where(c => c.Id == contactId && c.CustomerId == customerId)
            .Select(c => new UpdateContactRequest
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Position = c.Position,
                Note = c.Note,
                IsPrimary = c.IsPrimary,
                Version = c.Version
            })
            .SingleOrDefaultAsync(cancellationToken);

    /// <returns>The created contact, or null if the customer does not exist.</returns>
    public async Task<ContactResponse?> CreateAsync(
        int customerId,
        ContactRequest request,
        CancellationToken cancellationToken)
    {
        var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
        {
            return null;
        }

        var contact = new Contact
        {
            CustomerId = customerId,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        Apply(request, contact);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        if (contact.IsPrimary)
        {
            await ClearPrimaryContactAsync(customerId, cancellationToken);
        }

        db.Contacts.Add(contact);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ContactResponse.From(contact);
    }

    /// <returns>The updated contact, or null if it does not exist for this customer.</returns>
    /// <exception cref="DbUpdateConcurrencyException">The contact was changed by someone else.</exception>
    public async Task<ContactResponse?> UpdateAsync(
        int customerId,
        int contactId,
        UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await db.Contacts
            .SingleOrDefaultAsync(c => c.Id == contactId && c.CustomerId == customerId, cancellationToken);

        if (contact is null)
        {
            return null;
        }

        // Optimistic concurrency
        db.Entry(contact).Property(c => c.Version).OriginalValue = request.Version;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        if (request.IsPrimary && !contact.IsPrimary)
        {
            await ClearPrimaryContactAsync(customerId, cancellationToken);
        }

        Apply(request, contact);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ContactResponse.From(contact);
    }

    /// <summary>
    /// Permanently deletes the contact (personal data, GDPR right to erasure).
    /// </summary>
    public async Task<bool> DeleteAsync(int customerId, int contactId, CancellationToken cancellationToken)
    {
        var deletedRows = await db.Contacts
            .Where(c => c.Id == contactId && c.CustomerId == customerId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedRows > 0;
    }

    /// <summary>
    /// Takes the primary flag away from the current primary contact. Runs as its own UPDATE before
    /// the new primary contact is saved, so the unique index never sees two primary contacts.
    /// </summary>
    private Task<int> ClearPrimaryContactAsync(int customerId, CancellationToken cancellationToken) =>
        db.Contacts
            .Where(c => c.CustomerId == customerId && c.IsPrimary)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsPrimary, false)
                    // ExecuteUpdate skips SaveChanges, so we set the timestamp ourselves
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

    private static void Apply(ContactRequest request, Contact contact)
    {
        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Email = request.Email.NullIfEmpty();
        contact.Phone = request.Phone.NullIfEmpty();
        contact.Position = request.Position.NullIfEmpty();
        contact.Note = request.Note.NullIfEmpty();
        contact.IsPrimary = request.IsPrimary;
    }
}