using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_chat_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class chatStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatVersion",
                table: "ChatEdits",
                newName: "Version");

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "ChatMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatStates",
                columns: table => new
                {
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastMessageSeq = table.Column<long>(type: "bigint", nullable: false),
                    EditCursorId = table.Column<long>(type: "bigint", nullable: false),
                    UnreadCount = table.Column<int>(type: "integer", nullable: false),
                    LastPreview_AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastPreview_MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastPreview_SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPreview_Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatStates", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_ChatStates_ChatEdits_EditCursorId",
                        column: x => x.EditCursorId,
                        principalTable: "ChatEdits",
                        principalColumn: "Seq",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChatStates_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatUserStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastReadSeq = table.Column<long>(type: "bigint", nullable: false),
                    CachedUnreadCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUserStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatUserStates_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatStates_EditCursorId",
                table: "ChatStates",
                column: "EditCursorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUserStates_ChatId",
                table: "ChatUserStates",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatStates");

            migrationBuilder.DropTable(
                name: "ChatUserStates");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "ChatEdits",
                newName: "ChatVersion");
        }
    }
}
