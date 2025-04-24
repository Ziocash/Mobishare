using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the Report table in the database.
/// </summary>
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

        builder.Property(x => x.Image);

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

