using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class renameLogsIdsToSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentLogs",
                table: "ContentLogs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContentLogs");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AccessLogs",
                newName: "Seq");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Nodes"" 
                  ALTER COLUMN ""CursorId"" DROP DEFAULT;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Nodes"" 
                ALTER COLUMN ""CursorId"" TYPE bigint 
                USING (""CursorId""::text::bigint);");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Nodes"" 
                   ALTER COLUMN ""CursorId"" SET DEFAULT 0;");

            migrationBuilder.AddColumn<long>(
                name: "Seq",
                table: "ContentLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentLogs",
                table: "ContentLogs",
                column: "Seq");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes",
                column: "CursorId",
                principalTable: "ContentLogs",
                principalColumn: "Seq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentLogs",
                table: "ContentLogs");

            migrationBuilder.DropColumn(
                name: "Seq",
                table: "ContentLogs");

            migrationBuilder.RenameColumn(
                name: "Seq",
                table: "AccessLogs",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "CursorId",
                table: "Nodes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ContentLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentLogs",
                table: "ContentLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_ContentLogs_CursorId",
                table: "Nodes",
                column: "CursorId",
                principalTable: "ContentLogs",
                principalColumn: "Id");
        }
    }
}
