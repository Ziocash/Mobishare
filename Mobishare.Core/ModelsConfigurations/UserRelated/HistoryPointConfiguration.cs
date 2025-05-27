using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.ModelsConfigurations.UserRelated;

public class HistoryPointConfiguration : IEntityTypeConfiguration<HistoryPoint>
{
    public void Configure(EntityTypeBuilder<HistoryPoint> builder)
    {
        builder.ToTable("HistoryPoints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Point)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .IsRequired();

        builder.HasOne(x => x.Balance)
                .WithMany()
                .HasForeignKey(x => x.BalanceId)
                .IsRequired();
                
        builder.Property(x => x.TransactionType)
                .IsRequired();
    }
}
