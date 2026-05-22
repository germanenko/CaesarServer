using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;
using System.Reflection.Emit;

namespace planner_node_service.Infrastructure.Configurations
{
    public class NodeConfiguration : IEntityTypeConfiguration<Node>
    {
        public void Configure(EntityTypeBuilder<Node> builder)
        {
            builder.ToTable("Nodes");

            //builder.HasOne(e => e.Cursor)
            //    .WithMany()
            //    .HasForeignKey(e => e.CursorId)
            //    .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Version)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.SyncKind)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Props)
                .HasColumnType("jsonb");

            //! убрал до задачи поисковых индексов
            //builder.HasGeneratedTsVectorColumn(
            //    p => p.SearchVector,
            //    "simple",
            //    p => new
            //    {
            //        p.BodyJson
            //    });

            //builder
            //    .HasIndex(p => p.SearchVector)
            //    .HasMethod("GIN");

            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.Name);

        }
    }
}
