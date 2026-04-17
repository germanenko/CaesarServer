using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_content_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Infrastructure.Configurations
{
    public class AttachedMessageConfiguration : IEntityTypeConfiguration<AttachedMessage>
    {
        public void Configure(EntityTypeBuilder<AttachedMessage> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(x => x.Job)
                .WithMany(j => j.AttachedMessages)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.JobId, x.MessageId })
                .IsUnique();
        }
    }
}
