using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class cursorInTrackables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CursorId",
                table: "Trackables",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Trackables_CursorId",
                table: "Trackables",
                column: "CursorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trackables_ContentLogs_CursorId",
                table: "Trackables",
                column: "CursorId",
                principalTable: "ContentLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trackables_ContentLogs_CursorId",
                table: "Trackables");

            migrationBuilder.DropIndex(
                name: "IX_Trackables_CursorId",
                table: "Trackables");

            migrationBuilder.DropColumn(
                name: "CursorId",
                table: "Trackables");
        }
    }
}
