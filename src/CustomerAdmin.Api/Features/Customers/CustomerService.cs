using CustomerAdmin.Api.Common;
using CustomerAdmin.Api.Data;
using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerAdmin.Api.Features.Customers;

public sealed class CustomerService(CustomerAdminDbContext db)
{
    public async Task<IReadOnlyList<CustomerSummaryResponse>> GetListAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = db.Customers.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Finds customers by their own name or by the name of one of their contacts
            var pattern = SearchPatterns.Contains(search);
            query = query.Where(c =>
                EF.Functions.ILike(c.Name, pattern, SearchPatterns.EscapeCharacter) ||
                c.Contacts.Any(x => EF.Functions.ILike(
                    x.FirstName + " " + x.LastName, pattern, SearchPatterns.EscapeCharacter)));
        }

        // Projection: only the needed columns; the contact count becomes a COUNT subquery
        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerSummaryResponse(
                c.Id,
                c.Name,
                c.City,
                c.Email,
                c.Phone,
                c.IsActive,
                c.Contacts.Count,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .Include(c => c.Contacts)
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

        return customer is null ? null : CustomerDetailResponse.From(customer);
    }

    /// <summary>
    /// Returns the current values of a customer as an update request. Used as the target for JSON Patch,
    /// so a patch goes through the same validation and update logic as PUT.
    /// </summary>
    public async Task<UpdateCustomerRequest?> GetUpdateRequestAsync(int id, CancellationToken cancellationToken) =>
        await db.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new UpdateCustomerRequest
            {
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Street = c.Street,
                PostalCode = c.PostalCode,
                City = c.City,
                Note = c.Note,
                IsActive = c.IsActive,
                Version = c.Version
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CustomerDetailResponse> CreateAsync(
        CustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer { Name = request.Name };
        Apply(request, customer);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return CustomerDetailResponse.From(customer);
    }

    /// <returns>The updated customer, or null if it does not exist.</returns>
    /// <exception cref="DbUpdateConcurrencyException">The customer was changed by someone else.</exception>
    public async Task<CustomerDetailResponse?> UpdateAsync(
        int id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .Include(c => c.Contacts)
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return null;
        }

        // Optimistic concurrency
        db.Entry(customer).Property(c => c.Version).OriginalValue = request.Version;

        Apply(request, customer);
        customer.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return CustomerDetailResponse.From(customer);
    }

    /// <summary>
    /// Permanently deletes the customer. The database deletes its contacts as well (cascade).
    /// </summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deletedRows = await db.Customers
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedRows > 0;
    }

    private static void Apply(CustomerRequest request, Customer customer)
    {
        customer.Name = request.Name.Trim();
        customer.Email = request.Email.NullIfEmpty();
        customer.Phone = request.Phone.NullIfEmpty();
        customer.Street = request.Street.NullIfEmpty();
        customer.PostalCode = request.PostalCode.NullIfEmpty();
        customer.City = request.City.NullIfEmpty();
        customer.Note = request.Note.NullIfEmpty();
    }
}