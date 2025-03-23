using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.User;

namespace Mobishare.Core.ModelsConfigurations.User;

//<summary>
// This class is used to configure the Balances table in the database.
//</summary>
public class BalancesConfiguration : IEntityTypeConfiguration<Balances>
{
    public void Configure(EntityTypeBuilder<Balances> builder)
    {
        builder.ToTable("Balances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Credit).IsRequired();
        builder.Property(x => x.Points).IsRequired();
        builder.HasOne(x => x.User)
               .WithOne()
               .HasForeignKey<Balances>(x => x.UserId)
               .IsRequired();
    }
}
