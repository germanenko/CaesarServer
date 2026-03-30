using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_chat_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class MessagesSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnreadCount",
                table: "ChatStates");

            migrationBuilder.AlterColumn<long>(
                name: "CachedUnreadCount",
                table: "ChatUserStates",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "Seq",
                table: "ChatMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seq",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<int>(
                name: "CachedUnreadCount",
                table: "ChatUserStates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "UnreadCount",
                table: "ChatStates",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
