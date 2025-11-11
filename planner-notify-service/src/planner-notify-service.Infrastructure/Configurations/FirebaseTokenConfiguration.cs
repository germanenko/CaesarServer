using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_notify_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<FirebaseToken>
    {
        public void Configure(EntityTypeBuilder<FirebaseToken> builder)
        {
            builder.HasKey(e => new { e.UserId, e.Token });

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Token);

            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Token).IsRequired().HasMaxLength(500);
        }
    }
}
