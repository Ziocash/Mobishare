using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the Ride table in the database.
/// </summary>
public class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.ToTable("Rides");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name);

        builder.Property(x => x.StartDateTime)
                .IsRequired();

        builder.Property(x => x.EndDateTime)
                .IsRequired();

        builder.Property(x => x.Price)
                .IsRequired();

        builder.HasOne(r => r.PositionStart)
                .WithMany()
                .HasForeignKey(r => r.PositionStartId);

        builder.HasOne(r => r.PositionEnd)
                .WithMany()
                .HasForeignKey(r => r.PositionEndId);

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
