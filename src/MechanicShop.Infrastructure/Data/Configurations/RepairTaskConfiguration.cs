using MechanicShop.Domain.RepairTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;


public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.ToTable("RepairTasks");

        builder.HasKey(r => r.Id)
            .HasAnnotation("SqlServer:Clustered", false);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.EstimatedDurationInMins)
            .HasConversion<string>();

        builder.Property(r => r.EstimatedDurationInMins)
            .IsRequired();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LaborCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasMany(r => r.Parts)
            .WithOne()
            .HasForeignKey("RepairTaskId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Parts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}