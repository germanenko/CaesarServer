using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planer_task_board.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PriorityOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HexColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDraft = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DraftOfTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Tasks_DraftOfTaskId",
                        column: x => x.DraftOfTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardColumns_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardMembers",
                columns: table => new
                {
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMembers", x => new { x.BoardId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_BoardMembers_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeletedTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExistBeforeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeletedTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskAttachedMessage",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAttachedMessage", x => new { x.TaskId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_TaskAttachedMessage_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskPerformers",
                columns: table => new
                {
                    PerformerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPerformers", x => new { x.PerformerId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_TaskPerformers_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardColumnMembers",
                columns: table => new
                {
                    ColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumnMembers", x => new { x.ColumnId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_BoardColumnMembers_BoardColumns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardColumnTasks",
                columns: table => new
                {
                    ColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumnTasks", x => new { x.ColumnId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_BoardColumnTasks_BoardColumns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardColumnTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId",
                table: "BoardColumns",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumnTasks_TaskId",
                table: "BoardColumnTasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskPerformers_TaskId",
                table: "TaskPerformers",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DraftOfTaskId",
                table: "Tasks",
                column: "DraftOfTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardColumnMembers");

            migrationBuilder.DropTable(
                name: "BoardColumnTasks");

            migrationBuilder.DropTable(
                name: "BoardMembers");

            migrationBuilder.DropTable(
                name: "DeletedTasks");

            migrationBuilder.DropTable(
                name: "TaskAttachedMessage");

            migrationBuilder.DropTable(
                name: "TaskPerformers");

            migrationBuilder.DropTable(
                name: "BoardColumns");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Boards");
        }
    }
}
