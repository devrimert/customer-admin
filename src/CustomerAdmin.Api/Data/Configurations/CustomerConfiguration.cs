using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerAdmin.Api.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(254);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Street).HasMaxLength(200);
        builder.Property(c => c.PostalCode).HasMaxLength(20);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Note).HasMaxLength(2000);

        // The customer list is sorted by name
        builder.HasIndex(c => c.Name);
    }
}