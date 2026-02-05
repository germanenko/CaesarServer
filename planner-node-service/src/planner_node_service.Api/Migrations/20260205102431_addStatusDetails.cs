using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class addStatusDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StatusDetailsId",
                table: "Statuses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StatusDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStatusDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStatusDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStatusDetails_StatusDetails_Id",
                        column: x => x.Id,
                        principalTable: "StatusDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_StatusDetailsId",
                table: "Statuses",
                column: "StatusDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statuses_StatusDetails_StatusDetailsId",
                table: "Statuses",
                column: "StatusDetailsId",
                principalTable: "StatusDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statuses_StatusDetails_StatusDetailsId",
                table: "Statuses");

            migrationBuilder.DropTable(
                name: "WorkflowStatusDetails");

            migrationBuilder.DropTable(
                name: "StatusDetails");

            migrationBuilder.DropIndex(
                name: "IX_Statuses_StatusDetailsId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "StatusDetailsId",
                table: "Statuses");
        }
    }
}
