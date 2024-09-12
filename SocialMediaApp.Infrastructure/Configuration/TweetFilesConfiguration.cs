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
    public class TweetFilesConfiguration : IEntityTypeConfiguration<TweetFiles>
    {
        public void Configure(EntityTypeBuilder<TweetFiles> builder)
        {
            builder.HasKey(x=>x.TweetFilesID);
            builder.Property(x => x.TweetFilesID).ValueGeneratedNever();
            builder.ToTable("TweetFiles");
        }
    }
}