using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forum.Data.Configurations;

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.ToTable("CommentReactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).IsRequired();

        builder.HasIndex(x => new { x.CommentId, x.UserId, x.Type }).IsUnique();

        builder.HasOne(x => x.Comment)
            .WithMany(x => x.Reactions)
            .HasForeignKey(x => x.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.CommentReactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
