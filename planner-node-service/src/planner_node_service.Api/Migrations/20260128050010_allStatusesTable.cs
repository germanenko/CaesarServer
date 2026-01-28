using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class allStatusesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationStatuses_Nodes_NodeId",
                table: "PublicationStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStatuses_Nodes_NodeId",
                table: "WorkflowStatuses");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStatuses_NodeId",
                table: "WorkflowStatuses");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStatuses_NodeId_UpdatedAt",
                table: "WorkflowStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PublicationStatuses_NodeId",
                table: "PublicationStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PublicationStatuses_NodeId_UpdatedAt",
                table: "PublicationStatuses");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "WorkflowStatuses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkflowStatuses");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "PublicationStatuses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PublicationStatuses");

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statuses_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_NodeId",
                table: "Statuses",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationStatuses_Statuses_Id",
                table: "PublicationStatuses",
                column: "Id",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStatuses_Statuses_Id",
                table: "WorkflowStatuses",
                column: "Id",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationStatuses_Statuses_Id",
                table: "PublicationStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStatuses_Statuses_Id",
                table: "WorkflowStatuses");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.AddColumn<Guid>(
                name: "NodeId",
                table: "WorkflowStatuses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkflowStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "NodeId",
                table: "PublicationStatuses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PublicationStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_NodeId",
                table: "WorkflowStatuses",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_NodeId_UpdatedAt",
                table: "WorkflowStatuses",
                columns: new[] { "NodeId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationStatuses_NodeId",
                table: "PublicationStatuses",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationStatuses_NodeId_UpdatedAt",
                table: "PublicationStatuses",
                columns: new[] { "NodeId", "UpdatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationStatuses_Nodes_NodeId",
                table: "PublicationStatuses",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStatuses_Nodes_NodeId",
                table: "WorkflowStatuses",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
