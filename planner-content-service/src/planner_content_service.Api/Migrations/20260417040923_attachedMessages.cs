using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class attachedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageSnapshot",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageState",
                table: "Jobs");

            migrationBuilder.CreateTable(
                name: "AttachedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Snapshot = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachedMessages_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PrimarySourceMessageId",
                table: "Jobs",
                column: "PrimarySourceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachedMessages_JobId",
                table: "AttachedMessages",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_AttachedMessages_PrimarySourceMessageId",
                table: "Jobs",
                column: "PrimarySourceMessageId",
                principalTable: "AttachedMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_AttachedMessages_PrimarySourceMessageId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "AttachedMessages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PrimarySourceMessageId",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "PrimarySourceMessageSnapshot",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrimarySourceMessageState",
                table: "Jobs",
                type: "integer",
                nullable: true);
        }
    }
}
