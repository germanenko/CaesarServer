using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class deleteTrackables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Trackables_TrackableId",
                table: "History");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Trackables_Id",
                table: "Nodes");

            migrationBuilder.DropTable(
                name: "Trackables");

            migrationBuilder.RenameColumn(
                name: "TrackableId",
                table: "History",
                newName: "NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_TrackableId",
                table: "History",
                newName: "IX_Histories_NodeId");

            migrationBuilder.AddColumn<Guid>(
                name: "CursorId",
                table: "Nodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Nodes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CursorId",
                table: "Nodes",
                column: "CursorId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Nodes_NodeId",
                table: "History",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes",
                column: "CursorId",
                principalTable: "ContentLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Nodes_NodeId",
                table: "History");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_CursorId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "CursorId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Nodes");

            migrationBuilder.RenameColumn(
                name: "NodeId",
                table: "History",
                newName: "TrackableId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_NodeId",
                table: "History",
                newName: "IX_Histories_TrackableId");

            migrationBuilder.CreateTable(
                name: "Trackables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CursorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trackables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trackables_ContentLogs_CursorId",
                        column: x => x.CursorId,
                        principalTable: "ContentLogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trackables_CursorId",
                table: "Trackables",
                column: "CursorId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Trackables_TrackableId",
                table: "History",
                column: "TrackableId",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Trackables_Id",
                table: "Nodes",
                column: "Id",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
