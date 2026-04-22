using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class readStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttachedMessages_JobId",
                table: "AttachedMessages");

            migrationBuilder.AddColumn<DateTime>(
                name: "AttachedAtUtc",
                table: "AttachedMessages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ReadStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastReadAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachedMessages_JobId_MessageId",
                table: "AttachedMessages",
                columns: new[] { "JobId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReadStates");

            migrationBuilder.DropIndex(
                name: "IX_AttachedMessages_JobId_MessageId",
                table: "AttachedMessages");

            migrationBuilder.DropColumn(
                name: "AttachedAtUtc",
                table: "AttachedMessages");

            migrationBuilder.CreateIndex(
                name: "IX_AttachedMessages_JobId",
                table: "AttachedMessages",
                column: "JobId");
        }
    }
}
