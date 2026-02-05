using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class deleteStatusTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationStatuses");

            migrationBuilder.DropTable(
                name: "WorkflowStatuses");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "StatusHistory",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Actor",
                table: "StatusHistory",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "History",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ActorId",
                table: "History",
                newName: "UpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_ActorId",
                table: "History",
                newName: "IX_Histories_UpdatedById");

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "Statuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Statuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NewVersion",
                table: "History",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldVersion",
                table: "History",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "NewVersion",
                table: "History");

            migrationBuilder.DropColumn(
                name: "OldVersion",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "StatusHistory",
                newName: "Actor");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "StatusHistory",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "History",
                newName: "ActorId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "History",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_UpdatedById",
                table: "History",
                newName: "IX_Histories_ActorId");

            migrationBuilder.CreateTable(
                name: "PublicationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationStatuses_Statuses_Id",
                        column: x => x.Id,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStatuses_Statuses_Id",
                        column: x => x.Id,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
