using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.ModelsConfigurations.Maps;

//<summary>
// This class is used to configure City table in the database.
//</summary>
public class CitiesConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(x => x.PerimeterLocation)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.HasOne(x => x.User)
               .WithOne()
               .HasForeignKey<City>(x => x.UserId)
               .IsRequired();
    }
}
