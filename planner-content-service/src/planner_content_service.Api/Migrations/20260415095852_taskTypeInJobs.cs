using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class taskTypeInJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobType",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");
        }
    }
}
