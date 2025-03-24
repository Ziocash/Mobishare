using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Map;

namespace Mobishare.Core.ModelsConfigurations.Map;

public class CitiesConfiguration : IEntityTypeConfiguration<Cities>
{
    public void Configure(EntityTypeBuilder<Cities> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.PermiterLocation)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.HasOne(x => x.User)
               .WithOne()
               .HasForeignKey<Cities>(x => x.UserId)
               .IsRequired();
    }
}
