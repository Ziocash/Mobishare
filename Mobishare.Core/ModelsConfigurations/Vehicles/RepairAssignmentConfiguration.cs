using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the RepairAssignment table in the database.
/// </summary>
public class RepairAssignmentConfiguration : IEntityTypeConfiguration<RepairAssignment>
{
    public void Configure(EntityTypeBuilder<RepairAssignment> builder)
    {
        builder.ToTable("RepairAssignments");

        builder.HasKey(x => new { x.UserId, x.RepairId });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .IsRequired();

        builder.HasOne(x => x.Repair)
               .WithMany()
               .HasForeignKey(x => x.RepairId)
               .IsRequired();
    }
}
