using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace planner_node_service.Api.Migrations
{
    /// <inheritdoc />
    public partial class versionAutoincTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION set_content_log_version()
                RETURNS trigger AS $$
                BEGIN
                  NEW.""Version"" := COALESCE(
                    (
                      SELECT ""Version""
                      FROM ""ContentLogs""
                      WHERE ""EntityId"" = NEW.""EntityId""
                      ORDER BY ""Version"" DESC
                      LIMIT 1
                    ),
                    0
                  ) + 1;
                
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_set_content_log_version ON ""ContentLogs"";

                CREATE TRIGGER trg_set_content_log_version
                BEFORE INSERT ON ""ContentLogs""
                FOR EACH ROW
                EXECUTE FUNCTION set_content_log_version();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_set_content_log_version ON ""ContentLogs"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS set_content_log_version;");
        }
    }
}
