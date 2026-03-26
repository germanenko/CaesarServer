using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_chat_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class userHiddenMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHiddenMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHiddenMessages", x => new { x.MessageId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_UserHiddenMessages_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHiddenMessages");
        }
    }
}
