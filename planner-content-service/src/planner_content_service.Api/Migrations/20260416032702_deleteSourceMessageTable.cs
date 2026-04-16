using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class deleteSourceMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_SourceMessages_message_source_id",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "SourceMessages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_message_source_id",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "message_source_id",
                table: "Jobs",
                newName: "PrimarySourceMessageId");

            migrationBuilder.AddColumn<int>(
                name: "PrimarySourceMessageState",
                table: "Jobs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageState",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "PrimarySourceMessageId",
                table: "Jobs",
                newName: "message_source_id");

            migrationBuilder.CreateTable(
                name: "SourceMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageState = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceMessages", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_message_source_id",
                table: "Jobs",
                column: "message_source_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_SourceMessages_message_source_id",
                table: "Jobs",
                column: "message_source_id",
                principalTable: "SourceMessages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
