using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Map;

namespace Mobishare.Core.ModelsConfigurations.Map;

public class ParkingSlotsConfiguration : IEntityTypeConfiguration<ParkingSlots>
{
    public void Configure(EntityTypeBuilder<ParkingSlots> builder)
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
               .HasForeignKey<ParkingSlots>(x => x.UserId)
               .IsRequired();
    }
}
