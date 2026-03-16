using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class newAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupMembers_AccessGroups_AccessGroupId",
                table: "AccessGroupMembers");

            migrationBuilder.DropTable(
                name: "AccessRights");

            migrationBuilder.DropTable(
                name: "AccessGroups");

            migrationBuilder.DropIndex(
                name: "IX_AccessGroupMembers_AccessGroupId",
                table: "AccessGroupMembers");

            migrationBuilder.RenameColumn(
                name: "AccessGroupId",
                table: "AccessGroupMembers",
                newName: "GroupId");

            migrationBuilder.CreateTable(
                name: "AccessSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncScopeAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    RulesRevisionUsed = table.Column<long>(type: "bigint", nullable: false),
                    GraphRevisionUsed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncScopeAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    RulesRevision = table.Column<long>(type: "bigint", nullable: false),
                    GraphRevision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLogs_AccessSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "AccessSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRules_AccessSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "AccessSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessRules_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessRules_Trackables_Id",
                        column: x => x.Id,
                        principalTable: "Trackables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupAccessSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAccessSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupAccessSubjects_AccessSubjects_Id",
                        column: x => x.Id,
                        principalTable: "AccessSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccessSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccessSubjects_AccessSubjects_Id",
                        column: x => x.Id,
                        principalTable: "AccessSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupMembers_GroupId_AccountId",
                table: "AccessGroupMembers",
                columns: new[] { "GroupId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_NodeId",
                table: "AccessLogs",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_SubjectId",
                table: "AccessLogs",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRules_NodeId",
                table: "AccessRules",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRules_SubjectId",
                table: "AccessRules",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRules_SubjectId_NodeId_Permission",
                table: "AccessRules",
                columns: new[] { "SubjectId", "NodeId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncScopeAccess_ScopeId_AccountId",
                table: "SyncScopeAccess",
                columns: new[] { "ScopeId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessSubjects_AccountId",
                table: "UserAccessSubjects",
                column: "AccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupMembers_GroupAccessSubjects_GroupId",
                table: "AccessGroupMembers",
                column: "GroupId",
                principalTable: "GroupAccessSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupMembers_GroupAccessSubjects_GroupId",
                table: "AccessGroupMembers");

            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropTable(
                name: "AccessRules");

            migrationBuilder.DropTable(
                name: "GroupAccessSubjects");

            migrationBuilder.DropTable(
                name: "SyncScopeAccess");

            migrationBuilder.DropTable(
                name: "UserAccessSubjects");

            migrationBuilder.DropTable(
                name: "AccessSubjects");

            migrationBuilder.DropIndex(
                name: "IX_AccessGroupMembers_GroupId_AccountId",
                table: "AccessGroupMembers");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "AccessGroupMembers",
                newName: "AccessGroupId");

            migrationBuilder.CreateTable(
                name: "AccessGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroups_Trackables_Id",
                        column: x => x.Id,
                        principalTable: "Trackables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessRights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Permission = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRights_AccessGroups_AccessGroupId",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessRights_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessRights_Trackables_Id",
                        column: x => x.Id,
                        principalTable: "Trackables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupMembers_AccessGroupId",
                table: "AccessGroupMembers",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRights_AccessGroupId",
                table: "AccessRights",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRights_AccountId",
                table: "AccessRights",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRights_AccountId_NodeId_Permission",
                table: "AccessRights",
                columns: new[] { "AccountId", "NodeId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessRights_NodeId",
                table: "AccessRights",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupMembers_AccessGroups_AccessGroupId",
                table: "AccessGroupMembers",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
