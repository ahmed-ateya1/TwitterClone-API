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
    public class RetweetConfiguration : IEntityTypeConfiguration<Retweet>
    {
        public void Configure(EntityTypeBuilder<Retweet> builder)
        {
            builder.HasKey(x => x.RetweetID);

            builder.Property(x => x.RetweetID)
                .ValueGeneratedNever();

            builder.ToTable("Retweets");
        }
    }
}