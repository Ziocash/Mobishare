using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.ModelsConfigurations.Chats;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Embedding)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.Property(x => x.Message)
                .IsRequired();
        
        builder.Property(x => x.Sender)
                .IsRequired();    
        
        builder.HasOne(x => x.Conversation)
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .IsRequired();

    }
}
