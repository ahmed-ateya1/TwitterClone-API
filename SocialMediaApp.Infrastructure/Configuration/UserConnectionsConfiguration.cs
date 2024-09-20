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
    public class UserConnectionsConfiguration : IEntityTypeConfiguration<UserConnections>
    {
        public void Configure(EntityTypeBuilder<UserConnections> builder)
        {
            builder.HasKey(x => x.UserConnectionID);

            builder.Property(x => x.UserConnectionID)
                .ValueGeneratedNever();
            builder.HasOne(x => x.Profile)
                .WithMany(x => x.UserConnections)
                .HasForeignKey(x=>x.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("UserConnections");
        }
    }
}