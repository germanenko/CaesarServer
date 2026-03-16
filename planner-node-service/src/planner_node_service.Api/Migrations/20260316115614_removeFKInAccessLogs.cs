using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeFKInAccessLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_AccessSubjects_SubjectId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Nodes_NodeId",
                table: "AccessLogs");

            migrationBuilder.DropIndex(
                name: "IX_AccessLogs_NodeId",
                table: "AccessLogs");

            migrationBuilder.DropIndex(
                name: "IX_AccessLogs_SubjectId",
                table: "AccessLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_NodeId",
                table: "AccessLogs",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_SubjectId",
                table: "AccessLogs",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_AccessSubjects_SubjectId",
                table: "AccessLogs",
                column: "SubjectId",
                principalTable: "AccessSubjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Nodes_NodeId",
                table: "AccessLogs",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}
