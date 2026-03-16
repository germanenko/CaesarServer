using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeTrackLinksAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRules_Trackables_Id",
                table: "AccessRules");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Trackables_Id",
                table: "NodeLinks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AccessRules_Trackables_Id",
                table: "AccessRules",
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
    }
}
