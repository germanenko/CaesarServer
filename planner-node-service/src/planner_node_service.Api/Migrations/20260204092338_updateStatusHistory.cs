using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistory_Statuses_StatusId",
                table: "StatusHistory");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StatusHistory",
                newName: "OldStatusId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "StatusHistory",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "StatusHistory",
                newName: "NewStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_StatusHistory_StatusId",
                table: "StatusHistory",
                newName: "IX_StatusHistory_NewStatusId");

            migrationBuilder.AddColumn<Guid>(
                name: "Actor",
                table: "StatusHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_OldStatusId",
                table: "StatusHistory",
                column: "OldStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistory_Statuses_NewStatusId",
                table: "StatusHistory",
                column: "NewStatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistory_Statuses_OldStatusId",
                table: "StatusHistory",
                column: "OldStatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistory_Statuses_NewStatusId",
                table: "StatusHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistory_Statuses_OldStatusId",
                table: "StatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_StatusHistory_OldStatusId",
                table: "StatusHistory");

            migrationBuilder.DropColumn(
                name: "Actor",
                table: "StatusHistory");

            migrationBuilder.RenameColumn(
                name: "OldStatusId",
                table: "StatusHistory",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "NewStatusId",
                table: "StatusHistory",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "StatusHistory",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_StatusHistory_NewStatusId",
                table: "StatusHistory",
                newName: "IX_StatusHistory_StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistory_Statuses_StatusId",
                table: "StatusHistory",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
