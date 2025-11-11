using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_notify_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class changeFirebaseToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FirebaseTokens",
                table: "FirebaseTokens");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FirebaseTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "FirebaseTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FirebaseTokens",
                table: "FirebaseTokens",
                columns: new[] { "UserId", "Token" });

            migrationBuilder.CreateIndex(
                name: "IX_FirebaseTokens_Token",
                table: "FirebaseTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_FirebaseTokens_UserId",
                table: "FirebaseTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FirebaseTokens",
                table: "FirebaseTokens");

            migrationBuilder.DropIndex(
                name: "IX_FirebaseTokens_Token",
                table: "FirebaseTokens");

            migrationBuilder.DropIndex(
                name: "IX_FirebaseTokens_UserId",
                table: "FirebaseTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "FirebaseTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "FirebaseTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FirebaseTokens",
                table: "FirebaseTokens",
                column: "Id");
        }
    }
}
