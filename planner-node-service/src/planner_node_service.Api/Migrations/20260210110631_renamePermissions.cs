using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class renamePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccessType",
                table: "AccessRights",
                newName: "Permission");

            migrationBuilder.RenameIndex(
                name: "IX_AccessRights_AccountId_NodeId_AccessType",
                table: "AccessRights",
                newName: "IX_AccessRights_AccountId_NodeId_Permission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Permission",
                table: "AccessRights",
                newName: "AccessType");

            migrationBuilder.RenameIndex(
                name: "IX_AccessRights_AccountId_NodeId_Permission",
                table: "AccessRights",
                newName: "IX_AccessRights_AccountId_NodeId_AccessType");
        }
    }
}
