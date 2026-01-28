using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class TrackableConfiguration : IEntityTypeConfiguration<TrackableEntity>
    {
        public void Configure(EntityTypeBuilder<TrackableEntity> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
