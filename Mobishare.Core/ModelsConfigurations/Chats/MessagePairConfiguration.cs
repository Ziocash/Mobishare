using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.ModelsConfigurations.Chats;

public class MessagePairConfiguration : IEntityTypeConfiguration<MessagePair>
{
    public void Configure(EntityTypeBuilder<MessagePair> builder)
    {
        builder.ToTable("MessagePairs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsForRag)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .IsRequired();

        builder.Property(x => x.Answered)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired();

        builder.HasOne(x => x.UserMessage)
            .WithMany()
            .HasForeignKey(x => x.UserMessageId)
            .IsRequired(false);

        builder.HasOne(x => x.AiMessage)
            .WithMany()
            .HasForeignKey(x => x.AiMessageId);
    }
}
