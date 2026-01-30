using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Trackables_TrackableId",
                table: "Histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Histories",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_CreatedAt",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_CreatedAt_TrackableId",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_CreatedBy",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_TrackableId_CreatedAt",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Histories");

            migrationBuilder.RenameTable(
                name: "Histories",
                newName: "History");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "History",
                newName: "ActorId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "History",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_UpdatedBy",
                table: "History",
                newName: "IX_Histories_ActorId");

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "History",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_History",
                table: "History",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Trackables_TrackableId",
                table: "History",
                column: "TrackableId",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Trackables_TrackableId",
                table: "History");

            migrationBuilder.DropPrimaryKey(
                name: "PK_History",
                table: "History");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "History");

            migrationBuilder.RenameTable(
                name: "History",
                newName: "Histories");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Histories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ActorId",
                table: "Histories",
                newName: "UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_ActorId",
                table: "Histories",
                newName: "IX_Histories_UpdatedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Histories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Histories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Histories",
                table: "Histories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_CreatedAt",
                table: "Histories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_CreatedAt_TrackableId",
                table: "Histories",
                columns: new[] { "CreatedAt", "TrackableId" });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_CreatedBy",
                table: "Histories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_TrackableId_CreatedAt",
                table: "Histories",
                columns: new[] { "TrackableId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Trackables_TrackableId",
                table: "Histories",
                column: "TrackableId",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
