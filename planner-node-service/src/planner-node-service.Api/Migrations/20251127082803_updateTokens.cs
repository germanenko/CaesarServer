using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_notify_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FirebaseTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FirebaseTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FirebaseTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FirebaseTokens");
        }
    }
}
