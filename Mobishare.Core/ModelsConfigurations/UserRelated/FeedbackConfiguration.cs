using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.ModelsConfigurations.UserRelated;

//<summary>
// This class is used to configure the Feedback table in the database.
//</summary>
public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
                .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.Property(x => x.Rating)
                .IsRequired()
                .HasDefaultValue(0);
                
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .IsRequired();
    }
}
