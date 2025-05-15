using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .IsRequired();
    }
}
