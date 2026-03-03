using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_analytics_service.Core.Entities.Models;

namespace planner_analytics_service.Infrastructure.Configurations
{
    public class AnalyticsActionConfiguration : IEntityTypeConfiguration<AnalyticsAction>
    {
        public void Configure(EntityTypeBuilder<AnalyticsAction> builder)
        {
            builder.ToTable("ChatMessages");

            builder.Property(cm => cm.Level)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(cm => cm.Date)
                .IsRequired()
                .HasDefaultValueSql("NOW()");


            builder.HasIndex(cm => cm.Level)
                .HasDatabaseName("IX_AnalyticsAction_Level");
        }
    }
}