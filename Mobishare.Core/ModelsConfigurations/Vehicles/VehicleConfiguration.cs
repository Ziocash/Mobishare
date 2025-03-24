using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// Vehicle entity configuration.
/// </summary>
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Plate)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.Model)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.PricePerMinute)
                .IsRequired();
        
        builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.BatteryLevel)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.HasOne(x => x.ParkingSlot)
               .WithOne()
               .HasForeignKey<Vehicle>(x => x.ParkingSlotId)
               .IsRequired();
    }
}
