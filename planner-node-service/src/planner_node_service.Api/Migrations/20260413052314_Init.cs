using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ContentLogs",
                columns: table => new
                {
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeVersion = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentLogs", x => x.Seq);
                });

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldVersion = table.Column<string>(type: "text", nullable: true),
                    NewVersion = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Props = table.Column<string>(type: "jsonb", nullable: true),
                    SyncKind = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    RulesRevision = table.Column<long>(type: "bigint", nullable: false),
                    GraphRevision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.Seq);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Nodes_ScopeId",
                        column: x => x.ScopeId,
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
                });

            migrationBuilder.CreateTable(
                name: "NodeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeLinks_Nodes_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeLinks_Nodes_ParentId",
                        column: x => x.ParentId,
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
                name: "Statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_SyncScopeAccess_Nodes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroupMembers_GroupAccessSubjects_GroupId",
                        column: x => x.GroupId,
                        principalTable: "GroupAccessSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OldStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusHistory_Statuses_NewStatusId",
                        column: x => x.NewStatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatusHistory_Statuses_OldStatusId",
                        column: x => x.OldStatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupMembers_GroupId_AccountId",
                table: "AccessGroupMembers",
                columns: new[] { "GroupId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_ScopeId",
                table: "AccessLogs",
                column: "ScopeId");

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
                name: "IX_ContentLogs_EntityId",
                table: "ContentLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_NodeId",
                table: "History",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_UpdatedById",
                table: "History",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_ChildId",
                table: "NodeLinks",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_ParentId",
                table: "NodeLinks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_ParentId_ChildId_RelationType",
                table: "NodeLinks",
                columns: new[] { "ParentId", "ChildId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_RelationType",
                table: "NodeLinks",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Name",
                table: "Nodes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Type",
                table: "Nodes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_NodeId",
                table: "NotificationSettings",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_NodeId_Kind",
                table: "Statuses",
                columns: new[] { "NodeId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_NewStatusId",
                table: "StatusHistory",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_OldStatusId",
                table: "StatusHistory",
                column: "OldStatusId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessGroupMembers");

            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropTable(
                name: "AccessRules");

            migrationBuilder.DropTable(
                name: "ContentLogs");

            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropTable(
                name: "NodeLinks");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "StatusHistory");

            migrationBuilder.DropTable(
                name: "SyncScopeAccess");

            migrationBuilder.DropTable(
                name: "UserAccessSubjects");

            migrationBuilder.DropTable(
                name: "GroupAccessSubjects");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "AccessSubjects");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
