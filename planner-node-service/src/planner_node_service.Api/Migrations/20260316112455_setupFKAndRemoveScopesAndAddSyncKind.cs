using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class setupFKAndRemoveScopesAndAddSyncKind : Migration
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

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "ContentLogs",
                newName: "ScopeVersion");

            migrationBuilder.RenameIndex(
                name: "IX_ContentLogs_EntityId_Version",
                table: "ContentLogs",
                newName: "IX_ContentLogs_EntityId_ScopeVersion");

            migrationBuilder.AddColumn<int>(
                name: "SyncKind",
                table: "Nodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_AccessSubjects_SubjectId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Nodes_NodeId",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "SyncKind",
                table: "Nodes");

            migrationBuilder.RenameColumn(
                name: "ScopeVersion",
                table: "ContentLogs",
                newName: "Version");

            migrationBuilder.RenameIndex(
                name: "IX_ContentLogs_EntityId_ScopeVersion",
                table: "ContentLogs",
                newName: "IX_ContentLogs_EntityId_Version");

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scopes_Nodes_Id",
                        column: x => x.Id,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_AccessSubjects_SubjectId",
                table: "AccessLogs",
                column: "SubjectId",
                principalTable: "AccessSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
