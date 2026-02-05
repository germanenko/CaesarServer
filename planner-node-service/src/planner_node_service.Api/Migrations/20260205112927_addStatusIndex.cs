using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class addStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statuses_NodeId",
                table: "Statuses");

            migrationBuilder.CreateIndex(
                name: "IX_Status_NodeId_Kind",
                table: "Statuses",
                columns: new[] { "NodeId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Status_NodeId_Kind",
                table: "Statuses");

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_NodeId",
                table: "Statuses",
                column: "NodeId");
        }
    }
}
