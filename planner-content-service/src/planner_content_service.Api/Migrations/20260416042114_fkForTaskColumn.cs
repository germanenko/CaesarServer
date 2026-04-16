using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class fkForTaskColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTaskColumns_ColumnId",
                table: "UserTaskColumns",
                column: "ColumnId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskColumns_Columns_ColumnId",
                table: "UserTaskColumns",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskColumns_Columns_ColumnId",
                table: "UserTaskColumns");

            migrationBuilder.DropIndex(
                name: "IX_UserTaskColumns_ColumnId",
                table: "UserTaskColumns");
        }
    }
}
