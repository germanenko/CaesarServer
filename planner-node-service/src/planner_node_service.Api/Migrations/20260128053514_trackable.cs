using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class trackable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Nodes_NodeId",
                table: "Histories");

            migrationBuilder.RenameColumn(
                name: "NodeId",
                table: "Histories",
                newName: "TrackableId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_NodeId_CreatedAt",
                table: "Histories",
                newName: "IX_Histories_TrackableId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_NodeId",
                table: "Histories",
                newName: "IX_Histories_TrackableId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_CreatedAt_NodeId",
                table: "Histories",
                newName: "IX_Histories_CreatedAt_TrackableId");

            migrationBuilder.CreateTable(
                name: "Trackables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trackables", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Trackables_TrackableId",
                table: "Histories",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Trackables_TrackableId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Trackables_Id",
                table: "Nodes");

            migrationBuilder.DropTable(
                name: "Trackables");

            migrationBuilder.RenameColumn(
                name: "TrackableId",
                table: "Histories",
                newName: "NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_TrackableId_CreatedAt",
                table: "Histories",
                newName: "IX_Histories_NodeId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_TrackableId",
                table: "Histories",
                newName: "IX_Histories_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_CreatedAt_TrackableId",
                table: "Histories",
                newName: "IX_Histories_CreatedAt_NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Nodes_NodeId",
                table: "Histories",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
