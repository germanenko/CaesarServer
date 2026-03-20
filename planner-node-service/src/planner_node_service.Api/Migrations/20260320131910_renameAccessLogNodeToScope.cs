using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class renameAccessLogNodeToScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Nodes_NodeId",
                table: "AccessLogs");

            migrationBuilder.RenameColumn(
                name: "NodeId",
                table: "AccessLogs",
                newName: "ScopeId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLogs_NodeId",
                table: "AccessLogs",
                newName: "IX_AccessLogs_ScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Nodes_ScopeId",
                table: "AccessLogs",
                column: "ScopeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Nodes_ScopeId",
                table: "AccessLogs");

            migrationBuilder.RenameColumn(
                name: "ScopeId",
                table: "AccessLogs",
                newName: "NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_AccessLogs_ScopeId",
                table: "AccessLogs",
                newName: "IX_AccessLogs_NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Nodes_NodeId",
                table: "AccessLogs",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
