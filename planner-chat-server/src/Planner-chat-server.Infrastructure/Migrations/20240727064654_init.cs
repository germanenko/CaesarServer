using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planner_chat_server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMemberships_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountChatSessions_ChatMemberships_ChatMembershipId",
                        column: x => x.ChatMembershipId,
                        principalTable: "ChatMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountChatSessions_ChatMembershipId",
                table: "AccountChatSessions",
                column: "ChatMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountChatSessions_SessionId",
                table: "AccountChatSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMemberships_AccountId",
                table: "ChatMemberships",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMemberships_ChatId",
                table: "ChatMemberships",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatId",
                table: "ChatMessages",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountChatSessions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatMemberships");

            migrationBuilder.DropTable(
                name: "Chats");
        }
    }
}
