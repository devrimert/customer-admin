using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerAdmin.Api.Data;

public class CustomerAdminDbContext(DbContextOptions<CustomerAdminDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerAdminDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Sets audit timestamps in one central place, so no service has to remember it.
    /// </summary>
    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}