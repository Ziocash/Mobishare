using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Map;

namespace Mobishare.Core.ModelsConfigurations.Map;

public class CitiesConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
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
               .HasForeignKey<City>(x => x.UserId)
               .IsRequired();
    }
}
