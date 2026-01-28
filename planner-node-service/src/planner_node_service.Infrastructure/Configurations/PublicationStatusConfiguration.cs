using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using System.Reflection.Emit;

namespace planner_node_service.Infrastructure.Configurations
{
    public class PublicationStatusConfiguration : IEntityTypeConfiguration<PublicationStatusModel>
    {
        public void Configure(EntityTypeBuilder<PublicationStatusModel> builder)
        {
            builder.ToTable("PublicationStatuses");
        }
    }
}