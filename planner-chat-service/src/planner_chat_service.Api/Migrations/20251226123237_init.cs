using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_chat_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValue: "Chat Message"),
                    Props = table.Column<string>(type: "jsonb", nullable: true, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessGroupMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroupMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroupMember_AccessGroup_AccessGroupId",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessRights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccessGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<int>(type: "integer", nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRights_AccessGroup_AccessGroupId",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessRights_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    ChatType = table.Column<int>(type: "integer", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
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
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageType = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasBeenRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    MessageDraft = table.Column<string>(type: "text", nullable: true),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "AccountChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountChatSessions_ChatSettings_ChatSettingId",
                        column: x => x.ChatSettingId,
                        principalTable: "ChatSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupMember_AccessGroupId",
                table: "AccessGroupMember",
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
                name: "IX_AccountChatSessions_ChatSettingId",
                table: "AccountChatSessions",
                column: "ChatSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountChatSessions_SessionId",
                table: "AccountChatSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatId",
                table: "ChatMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_HasBeenRead",
                table: "ChatMessages",
                column: "HasBeenRead");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_MessageType",
                table: "ChatMessages",
                column: "MessageType");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_HasBeenRead_SentAt",
                table: "ChatMessages",
                columns: new[] { "SenderId", "HasBeenRead", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SentAt",
                table: "ChatMessages",
                column: "SentAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSettings_AccountId",
                table: "ChatSettings",
                column: "AccountId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessGroupMember");

            migrationBuilder.DropTable(
                name: "AccessRights");

            migrationBuilder.DropTable(
                name: "AccountChatSessions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "NodeLinks");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "AccessGroup");

            migrationBuilder.DropTable(
                name: "ChatSettings");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
