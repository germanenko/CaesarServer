using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class deleteStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationStatuses");

            migrationBuilder.DropTable(
                name: "WorkflowStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationStatuses_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStatuses_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationStatuses_NodeId",
                table: "PublicationStatuses",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationStatuses_NodeId_UpdatedAt",
                table: "PublicationStatuses",
                columns: new[] { "NodeId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_NodeId",
                table: "WorkflowStatuses",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_NodeId_UpdatedAt",
                table: "WorkflowStatuses",
                columns: new[] { "NodeId", "UpdatedAt" });
        }
    }
}
