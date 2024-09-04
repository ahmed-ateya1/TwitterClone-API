using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Configuration
{
    public class ConnectionProfileConfiguration : IEntityTypeConfiguration<ConnectionProfile>
    {
        public void Configure(EntityTypeBuilder<ConnectionProfile> builder)
        {
            builder.HasKey(cp => new { cp.ProfileID, cp.UserConnectionID });

            builder.ToTable("ConnectionProfiles");
        }
    }
}
