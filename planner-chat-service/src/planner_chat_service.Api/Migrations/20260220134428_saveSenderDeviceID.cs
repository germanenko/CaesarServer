using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_chat_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class saveSenderDeviceID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SenderDeviceId",
                table: "ChatMessages",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderDeviceId",
                table: "ChatMessages");
        }
    }
}
