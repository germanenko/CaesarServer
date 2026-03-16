using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeBodyJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyJson",
                table: "Nodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyJson",
                table: "Nodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
