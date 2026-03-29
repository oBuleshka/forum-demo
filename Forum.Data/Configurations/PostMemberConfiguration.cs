using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forum.Data.Configurations;

public class PostMemberConfiguration : IEntityTypeConfiguration<PostMember>
{
    public void Configure(EntityTypeBuilder<PostMember> builder)
    {
        builder.ToTable("PostMembers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.PostMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
