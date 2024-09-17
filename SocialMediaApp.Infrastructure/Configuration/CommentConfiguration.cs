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
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Tweet)
           .WithMany(x => x.Comments)
           .HasForeignKey(x => x.TweetID)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profile)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Comments");
    }
}
