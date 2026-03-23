using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeCursor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_CursorId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "CursorId",
                table: "Nodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CursorId",
                table: "Nodes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CursorId",
                table: "Nodes",
                column: "CursorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes",
                column: "CursorId",
                principalTable: "ContentLogs",
                principalColumn: "Seq");
        }
    }
}
