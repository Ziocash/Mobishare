using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the Repair table in the database.
/// </summary>
public class RepairConfiguration : IEntityTypeConfiguration<Repair>
{
    public void Configure(EntityTypeBuilder<Repair> builder)
    {
        builder.ToTable("Repairs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
                .IsRequired();

        builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

        builder.Property(x => x.FinishedAt);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .IsRequired();

        builder.HasOne(x => x.Vehicle)
               .WithMany()
               .HasForeignKey(x => x.VehicleId)
               .IsRequired();
    }
}

