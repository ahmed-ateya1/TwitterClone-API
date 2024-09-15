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
    public class CommentFilesConfiguration : IEntityTypeConfiguration<CommentFiles>
    {
        public void Configure(EntityTypeBuilder<CommentFiles> builder)
        {
            builder.HasKey(x => x.CommentFileID);

            builder.Property(x => x.CommentFileID).ValueGeneratedNever();

            builder.Property(x => x.FileUrl).IsRequired().HasMaxLength(255);

            builder.HasOne(x=>x.Comment)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.CommentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
