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
    public class UserTaskColumnConfiguration : IEntityTypeConfiguration<UserTaskColumn>
    {
        public void Configure(EntityTypeBuilder<UserTaskColumn> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(x => x.Column)
                .WithMany()
                .HasForeignKey(x => x.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
