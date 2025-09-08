using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ModelsConfigurations.Vehicles;

/// <summary>
/// This class is used to configure the ReportAssignment table in the database.
/// </summary>
public class ReportAssignmentConfiguration : IEntityTypeConfiguration<ReportAssignment>
{
    public void Configure(EntityTypeBuilder<ReportAssignment> builder)
    {
        builder.ToTable("ReportAssignments");

        builder.HasKey(x => new { x.UserId, x.ReportId });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .IsRequired();

        builder.HasOne(x => x.Report)
               .WithMany()
               .HasForeignKey(x => x.ReportId)
               .IsRequired();
    }
}
