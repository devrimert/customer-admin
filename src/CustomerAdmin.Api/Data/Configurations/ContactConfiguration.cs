using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerAdmin.Api.Data.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(254);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Position).HasMaxLength(100);
        builder.Property(c => c.Note).HasMaxLength(2000);

        // Optimistic concurrency
        builder.Property(c => c.Version)
            .IsRowVersion()
            .HasColumnName("xmin");

        builder.HasOne(c => c.Customer)
            .WithMany(c => c.Contacts)
            .HasForeignKey(c => c.CustomerId)
            // Deleting a customer permanently also deletes its contacts (personal data).
            // Deactivating (IsActive = false) is the normal, reversible way to hide a customer.
            .OnDelete(DeleteBehavior.Cascade);

        // Regular index for loading the contacts of a customer
        builder.HasIndex(c => c.CustomerId, "ix_contacts_customer_id")
            .HasDatabaseName("ix_contacts_customer_id");

        // Partial unique index: at most one primary contact per customer.
        // Only rows with is_primary = true are part of the index.
        // The database name is set explicitly: the API maps a violation of this index to a 409 response.
        builder.HasIndex(c => c.CustomerId, "ux_contacts_one_primary_per_customer")
            .IsUnique()
            .HasFilter("is_primary")
            .HasDatabaseName("ux_contacts_one_primary_per_customer");
    }
}