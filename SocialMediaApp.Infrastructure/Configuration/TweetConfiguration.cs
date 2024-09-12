using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Configuration
{
    public class TweetConfiguration : IEntityTypeConfiguration<Tweet>
    {
        public void Configure(EntityTypeBuilder<Tweet> builder)
        {
            builder.HasKey(x => x.TweetID);

            builder.Property(x => x.TweetID)
                .ValueGeneratedNever();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.TotalLikes)
                .IsRequired();

            builder.Property(x => x.TotalRetweets)
                .IsRequired();

            builder.Property(x => x.TotalComments)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
               .IsRequired();

            builder.HasOne(x => x.Genre)
                .WithMany(x => x.Tweets)
                .HasForeignKey(x => x.GenreID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Profile)
                .WithMany(x => x.Tweets)
                .HasForeignKey(x => x.ProfileID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Tweets");
        }
    }
}