using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class deleteNodeFromStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistory_Nodes_NodeId",
                table: "StatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_StatusHistory_NodeId",
                table: "StatusHistory");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "StatusHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NodeId",
                table: "StatusHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_NodeId",
                table: "StatusHistory",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistory_Nodes_NodeId",
                table: "StatusHistory",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
