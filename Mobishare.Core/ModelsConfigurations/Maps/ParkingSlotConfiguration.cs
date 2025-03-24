using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.ModelsConfigurations.Maps;

public class ParkingSlotsConfiguration : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable("ParkingSlots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PermiterLocation)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.HasOne(x => x.City)
               .WithMany()
               .HasForeignKey(x => x.CityId)
               .IsRequired();

        builder.HasOne(x => x.User)
               .WithOne()
               .HasForeignKey<ParkingSlot>(x => x.UserId)
               .IsRequired();
    }
}
