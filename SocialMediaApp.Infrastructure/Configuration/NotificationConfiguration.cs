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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.NotificationID);
            builder.Property(x => x.NotificationID)
                .ValueGeneratedNever();

            builder.HasOne(x=>x.Profile)
                .WithMany(x=>x.Notifications)
                .HasForeignKey(x=>x.ProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Notifications");
        }
    }
}