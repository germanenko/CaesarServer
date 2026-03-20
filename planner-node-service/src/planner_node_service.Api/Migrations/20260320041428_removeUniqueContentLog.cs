using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeUniqueContentLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentLogs_EntityId_ScopeVersion",
                table: "ContentLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ContentLogs_EntityId_ScopeVersion",
                table: "ContentLogs",
                columns: new[] { "EntityId", "ScopeVersion" },
                unique: true);
        }
    }
}
