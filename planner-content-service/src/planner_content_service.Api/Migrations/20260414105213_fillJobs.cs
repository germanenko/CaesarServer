using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class fillJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PermormerIds",
                table: "Tasks",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RemindAt",
                table: "Reminders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MeetAt",
                table: "Meetings",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MemberIds",
                table: "Meetings",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_RemindAt",
                table: "Reminders",
                column: "RemindAt");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_MeetAt",
                table: "Meetings",
                column: "MeetAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_RemindAt",
                table: "Reminders");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_MeetAt",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "PermormerIds",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RemindAt",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "MeetAt",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "MemberIds",
                table: "Meetings");
        }
    }
}
