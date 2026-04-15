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
    public class SourceMessageConfiguration : IEntityTypeConfiguration<SourceMessage>
    {
        public void Configure(EntityTypeBuilder<SourceMessage> builder)
        {
            builder.HasKey(e => e.MessageId);

            builder.Property(e => e.MessageState)
                .IsRequired()
                .HasConversion<int>();
        }
    }
}
