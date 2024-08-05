using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planner_Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RestoreCode = table.Column<string>(type: "text", nullable: true),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    RestoreCodeValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WasPasswordResetRequest = table.Column<bool>(type: "boolean", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    AuthenticationMethod = table.Column<string>(type: "text", nullable: false),
                    AuthorizationProvider = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: true),
                    TokenValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSessions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSessions_AccountId",
                table: "AccountSessions",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSessions");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
