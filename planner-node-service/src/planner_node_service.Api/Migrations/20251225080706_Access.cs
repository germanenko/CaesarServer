using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class Access : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupMember_AccessGroup_AccessGroupId",
                table: "AccessGroupMember");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRights_AccessGroup_AccessGroupId",
                table: "AccessRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessGroupMember",
                table: "AccessGroupMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessGroup",
                table: "AccessGroup");

            migrationBuilder.RenameTable(
                name: "AccessGroupMember",
                newName: "AccessGroupMembers");

            migrationBuilder.RenameTable(
                name: "AccessGroup",
                newName: "AccessGroups");

            migrationBuilder.RenameIndex(
                name: "IX_AccessGroupMember_AccessGroupId",
                table: "AccessGroupMembers",
                newName: "IX_AccessGroupMembers_AccessGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessGroupMembers",
                table: "AccessGroupMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessGroups",
                table: "AccessGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupMembers_AccessGroups_AccessGroupId",
                table: "AccessGroupMembers",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRights_AccessGroups_AccessGroupId",
                table: "AccessRights",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupMembers_AccessGroups_AccessGroupId",
                table: "AccessGroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRights_AccessGroups_AccessGroupId",
                table: "AccessRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessGroups",
                table: "AccessGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessGroupMembers",
                table: "AccessGroupMembers");

            migrationBuilder.RenameTable(
                name: "AccessGroups",
                newName: "AccessGroup");

            migrationBuilder.RenameTable(
                name: "AccessGroupMembers",
                newName: "AccessGroupMember");

            migrationBuilder.RenameIndex(
                name: "IX_AccessGroupMembers_AccessGroupId",
                table: "AccessGroupMember",
                newName: "IX_AccessGroupMember_AccessGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessGroup",
                table: "AccessGroup",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessGroupMember",
                table: "AccessGroupMember",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupMember_AccessGroup_AccessGroupId",
                table: "AccessGroupMember",
                column: "AccessGroupId",
                principalTable: "AccessGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRights_AccessGroup_AccessGroupId",
                table: "AccessRights",
                column: "AccessGroupId",
                principalTable: "AccessGroup",
                principalColumn: "Id");
        }
    }
}
