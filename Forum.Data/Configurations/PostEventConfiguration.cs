using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forum.Data.Configurations;

public class PostEventConfiguration : IEntityTypeConfiguration<PostEvent>
{
    public void Configure(EntityTypeBuilder<PostEvent> builder)
    {
        builder.ToTable("PostEvents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Post)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.PostEvents)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
