using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class cascadeToNodeLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_ParentId",
                table: "NodeLinks");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_ParentId",
                table: "NodeLinks",
                column: "ParentId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_ParentId",
                table: "NodeLinks");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_ParentId",
                table: "NodeLinks",
                column: "ParentId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
