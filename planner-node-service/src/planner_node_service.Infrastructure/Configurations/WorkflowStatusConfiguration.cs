using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class WorkflowStatusConfiguration : IEntityTypeConfiguration<WorkflowStatusModel>
    {
        public void Configure(EntityTypeBuilder<WorkflowStatusModel> builder)
        {
            builder.ToTable("WorkflowStatuses");
        }
    }
}