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

        builder.Property(x => x.CreatedAt)
                .HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(x => x.Report)
               .WithMany()
               .HasForeignKey(x => x.ReportId)
               .IsRequired();
    }
}

