using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_content_service.Api.Migrations.Idempotency
{
    /// <inheritdoc />
    public partial class EditNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "OperationFailures");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                table: "OperationFailures");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "OperationFailures",
                newName: "OperationFailureCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperationFailureCode",
                table: "OperationFailures",
                newName: "Kind");

            migrationBuilder.AddColumn<int>(
                name: "ErrorCode",
                table: "OperationFailures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ScopeId",
                table: "OperationFailures",
                type: "uuid",
                nullable: true);
        }
    }
}
