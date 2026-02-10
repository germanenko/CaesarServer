using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class clearTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessGroupMembers");

            migrationBuilder.DropTable(
                name: "AccessRights");

            migrationBuilder.DropTable(
                name: "AccountChatSession");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "NodeLinks");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "AccessGroups");

            migrationBuilder.DropTable(
                name: "ChatSettings");

            migrationBuilder.DropTable(
                name: "Chats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatType = table.Column<int>(type: "integer", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Nodes_Id",
                        column: x => x.Id,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
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
                name: "NodeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "AccessGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroupMembers_AccessGroups_AccessGroupId",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroups",
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
                    AccessType = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    HasBeenRead = table.Column<bool>(type: "boolean", nullable: false),
                    MessageType = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessages_Nodes_Id",
                        column: x => x.Id,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatName = table.Column<string>(type: "text", nullable: true),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MessageDraft = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSettings_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountChatSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountChatSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountChatSession_ChatSettings_ChatSettingId",
                        column: x => x.ChatSettingId,
                        principalTable: "ChatSettings",
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
                name: "IX_AccessRights_AccountId_NodeId_AccessType",
                table: "AccessRights",
                columns: new[] { "AccountId", "NodeId", "AccessType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessRights_NodeId",
                table: "AccessRights",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountChatSession_ChatSettingId",
                table: "AccountChatSession",
                column: "ChatSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatId",
                table: "ChatMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSettings_ChatId",
                table: "ChatSettings",
                column: "ChatId");

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
                name: "IX_NotificationSettings_NodeId",
                table: "NotificationSettings",
                column: "NodeId");
        }
    }
}
