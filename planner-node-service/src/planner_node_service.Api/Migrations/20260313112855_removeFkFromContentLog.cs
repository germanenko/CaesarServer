using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeFkFromContentLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentLogs_Trackables_EntityId",
                table: "ContentLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_ContentLogs_Trackables_EntityId",
                table: "ContentLogs",
                column: "EntityId",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
