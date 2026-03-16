using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class UserAccessSubjectConfiguration : IEntityTypeConfiguration<UserAccessSubject>
    {
        public void Configure(EntityTypeBuilder<UserAccessSubject> builder)
        {
            builder.HasIndex(e => new { e.AccountId })
                .IsUnique();
        }
    }
}
