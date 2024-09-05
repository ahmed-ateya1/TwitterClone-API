﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.Entites;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasKey(x => x.ProfileID);

        builder.Property(x => x.ProfileID)
            .ValueGeneratedNever();

        builder.Property(x => x.ProfileImgURL)
            .HasColumnType("VARCHAR(max)")
            .IsRequired();

        builder.Property(x => x.ProfileBackgroundURL)
            .HasColumnType("VARCHAR(max)")
            .IsRequired();

        builder.Property(x => x.Gender)
            .HasColumnType("VARCHAR(15)")
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .IsRequired();

        builder.Property(x => x.TotalFollowing)
            .IsRequired();

        builder.Property(x => x.TotalFollowers)
            .IsRequired();

        builder.Property(x => x.TotalTweets)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.Profile)
            .HasForeignKey<Profile>(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Retweets)
            .WithOne(x => x.Profile)
            .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.UserConnections)
            .WithMany(x => x.Profiles)
            .UsingEntity<ConnectionProfile>();

        builder.HasMany(x => x.Tweets)
            .WithOne(x => x.Profile)
             .HasForeignKey(x => x.ProfileID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Profiles");
    }
}