using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class trackAccessAndLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroups_Trackables_Id",
                table: "AccessGroups",
                column: "Id",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRights_Trackables_Id",
                table: "AccessRights",
                column: "Id",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Trackables_Id",
                table: "NodeLinks",
                column: "Id",
                principalTable: "Trackables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroups_Trackables_Id",
                table: "AccessGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRights_Trackables_Id",
                table: "AccessRights");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Trackables_Id",
                table: "NodeLinks");
        }
    }
}
