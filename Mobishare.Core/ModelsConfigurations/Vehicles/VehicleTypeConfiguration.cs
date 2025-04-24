using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the VehicleType entity in the database context.
/// </summary>
public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        builder.ToTable("VehicleTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PricePerMinute)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
