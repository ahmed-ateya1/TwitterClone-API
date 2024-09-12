using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.Entites;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(x => x.LikeID);

        builder.Property(x => x.LikeID)
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Comment)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.CommentID)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(x => x.Tweet)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.TweetID)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(x => x.Profile)
            .WithMany(x => x.Likes)
            .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.ToTable("Likes");
    }
}
