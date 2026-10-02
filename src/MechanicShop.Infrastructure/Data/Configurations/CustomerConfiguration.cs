using Microsoft.EntityFrameworkCore;
using MechanicShop.Domain.Customers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id).HasAnnotation("SqlServer:Clustered", false);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .IsRequired(false)
            .HasMaxLength(150);

        builder.HasMany(c => c.Vehicles).
            WithOne(v => v.Customer)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Vehicles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}