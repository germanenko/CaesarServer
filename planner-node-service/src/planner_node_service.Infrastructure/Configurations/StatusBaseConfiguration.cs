using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class StatusBaseConfiguration : IEntityTypeConfiguration<StatusBase>
    {
        public void Configure(EntityTypeBuilder<StatusBase> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
