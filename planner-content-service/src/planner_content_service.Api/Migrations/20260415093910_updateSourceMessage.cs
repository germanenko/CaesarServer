using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateSourceMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_MessageSnapshots_snapshot_id",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "MessageSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_snapshot_id",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageState",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "snapshot_id",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "PermormerIds",
                table: "Tasks",
                newName: "PerformerIds");

            migrationBuilder.AddColumn<string>(
                name: "PrimarySourceMessageSnapshot",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "message_source_id",
                table: "Jobs",
                type: "uuid",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_SourceMessages_message_source_id",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "SourceMessages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_message_source_id",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PrimarySourceMessageSnapshot",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "message_source_id",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "PerformerIds",
                table: "Tasks",
                newName: "PermormerIds");

            migrationBuilder.AddColumn<Guid>(
                name: "PrimarySourceMessageId",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PrimarySourceMessageState",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "snapshot_id",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MessageSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_snapshot_id",
                table: "Jobs",
                column: "snapshot_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_MessageSnapshots_snapshot_id",
                table: "Jobs",
                column: "snapshot_id",
                principalTable: "MessageSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
