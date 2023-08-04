using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddCacheTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"
                CREATE TABLE [dbo].[CobraCache]
                (
                    [Id] NVARCHAR(900) NOT NULL PRIMARY KEY, 
                    [Value] VARBINARY(MAX) NOT NULL, 
                    [ExpiresAtTime] DATETIMEOFFSET NOT NULL, 
                    [SlidingExpirationInSeconds] BIGINT NULL, 
                    [AbsoluteExpiration] DATETIMEOFFSET NULL
                )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"
                DROP TABLE [dbo].[CobraCache]
                (
                    [Id] NVARCHAR(900) NOT NULL PRIMARY KEY, 
                    [Value] VARBINARY(MAX) NOT NULL, 
                    [ExpiresAtTime] DATETIMEOFFSET NOT NULL, 
                    [SlidingExpirationInSeconds] BIGINT NULL, 
                    [AbsoluteExpiration] DATETIMEOFFSET NULL
                )
            ");
        }
    }
}
