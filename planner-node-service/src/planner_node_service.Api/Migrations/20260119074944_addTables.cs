using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class addTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Histories_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "IX_Histories_CreatedAt",
                table: "Histories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_CreatedAt_NodeId",
                table: "Histories",
                columns: new[] { "CreatedAt", "NodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_CreatedBy",
                table: "Histories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_NodeId",
                table: "Histories",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_NodeId_CreatedAt",
                table: "Histories",
                columns: new[] { "NodeId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_UpdatedBy",
                table: "Histories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_NodeId",
                table: "NotificationSettings",
                column: "NodeId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "PublicationStatuses");

            migrationBuilder.DropTable(
                name: "WorkflowStatuses");
        }
    }
}
