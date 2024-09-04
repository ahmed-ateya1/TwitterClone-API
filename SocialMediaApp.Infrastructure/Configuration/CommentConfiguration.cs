using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.Entites;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(x => x.CommentID);

        builder.Property(x => x.CommentID)
            .ValueGeneratedNever();

        builder.Property(x => x.TotalComment)
            .IsRequired();

        builder.Property(x => x.TotalLikes)
            .IsRequired();
        builder.Property(x => x.TotalRetweet)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Likes)
            .WithOne(x => x.Comment)
            .HasForeignKey(x => x.CommentID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Retweets)
            .WithOne(x => x.Comment)
            .HasForeignKey(x => x.CommentID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Comments");
    }
}
