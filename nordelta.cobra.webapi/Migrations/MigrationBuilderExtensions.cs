using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nordelta.cobra.webapi.Migrations
{
    public static class MigrationBuilderExtensions
    {
        // For readability, you should write sqlBaseStatement using string interpolation referencing this variables.
        // They'll take the correct values for each entity when needed.
        private const string PK_Property = "{2}", Table_Name = "{1}", Schema_Name = "{0}";

        /// <summary>
        /// IMPORTANT:
        /// This method needs to be called at the end of the migration's Up method.
        /// </summary>
        /// <param name="migrationBuilder"></param>
        /// <param name="targetModel"></param>
        public static void CreateAuditTriggersForAuditableEntities(this MigrationBuilder migrationBuilder, IModel targetModel)
        {
            #region SqlStatement
            // WARNING: This code doesn't check for sql injection. Be careful when reusing it for other purposes
            // There is no risk here because it is executed without user provided data.
            string SqlStatement = $@"
                      IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'{Schema_Name}.Tr_Audit_Update_On_{Table_Name}'))
                      DROP TRIGGER {Schema_Name}.Tr_Audit_Update_On_{Table_Name}
                      GO

                      CREATE TRIGGER {Schema_Name}.Tr_Audit_Update_On_{Table_Name}
                      ON {Schema_Name}.{Table_Name}
                      AFTER UPDATE
                      AS BEGIN
                          UPDATE {Schema_Name}.{Table_Name}
                          SET {RelationalDbContext.MODIFIEDON_PROPERTY} = GETDATE(), {RelationalDbContext.MODIFIEDBY_PROPERTY}=ORIGINAL_LOGIN()
                          FROM INSERTED i
                          WHERE {Schema_Name}.{Table_Name}.{PK_Property} = i.{PK_Property}
                      END
                      GO

                      IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'{Schema_Name}.Tr_Audit_Insert_On_{Table_Name}'))
                      DROP TRIGGER {Schema_Name}.Tr_Audit_Insert_On_{Table_Name}
                      GO

                      CREATE TRIGGER {Schema_Name}.Tr_Audit_Insert_On_{Table_Name}
                      ON {Schema_Name}.{Table_Name}
                      AFTER INSERT
                      AS BEGIN
                          UPDATE {Schema_Name}.{Table_Name}
                          SET {RelationalDbContext.CREATEDON_PROPERTY} = GETDATE(), {RelationalDbContext.CREATEDBY_PROPERTY}=ORIGINAL_LOGIN()
                          FROM INSERTED i
                          WHERE {Schema_Name}.{Table_Name}.{PK_Property} = i.{PK_Property}
                      END
                      GO
                  ";
            #endregion

            string FinalSqlStatement =
                GenerateSqlScriptForEachModelEntitieWithAttribute<AuditableAttribute>(targetModel, SqlStatement)
                .ToString();

            if (FinalSqlStatement != string.Empty)
            {
                migrationBuilder.Sql(FinalSqlStatement);
            }
        }



        public static void CreateAuditTable(this MigrationBuilder migrationBuilder)
        {
            string sqlScript = @"
                CREATE TABLE [AuditLogs]
                (
                    [Id] BIGINT IDENTITY(1,1) NOT NULL,
                    [StartDate] DATETIME NOT NULL,
                    [AuditLogType] NVARCHAR(50) NOT NULL,
				    [Data] NVARCHAR(MAX) NOT NULL,
                    [Duration] BIGINT NULL, 
					[ApiVersion] NVARCHAR(50) NULL,
					[ExceptionType] NVARCHAR(200) NULL,
					[HttpRequestTraceId] NVARCHAR(50) NULL,
					[HttpMethod] NVARCHAR(20) NULL,
					[HttpController] NVARCHAR(200) NULL,
					[HttpAction] NVARCHAR(200) NULL,
					[HttpRequestUrl] NVARCHAR(MAX) NULL,
                    [HttpResultCode] INT NULL,
                    [HttpRequestIp] NVARCHAR(45) NULL,
					[UserId] CHAR(38) NULL,
                    [UserEmail] NVARCHAR(254) NULL,
                	[Token] NVARCHAR(MAX) NULL,
                    CONSTRAINT PK_Audits PRIMARY KEY NONCLUSTERED (Id)
                )
                GO
                
                CREATE CLUSTERED INDEX idx_Audit_StartDate
                ON AuditLogs (StartDate);

				CREATE INDEX idx_Audit_Duration
                ON AuditLogs (Duration);
				CREATE INDEX idx_Audit_StartDate_Duration
                ON AuditLogs (StartDate, Duration);

				CREATE INDEX idx_Audit_HttpRequestTraceId
                ON AuditLogs (HttpRequestTraceId);
				CREATE INDEX idx_Audit_StartDate_HttpRequestTraceId
                ON AuditLogs (StartDate, HttpRequestTraceId);

				CREATE INDEX idx_Audit_ApiVersion
                ON AuditLogs (ApiVersion);
				CREATE INDEX idx_Audit_StartDate_ApiVersion
                ON AuditLogs (StartDate, ApiVersion);

				CREATE INDEX idx_Audit_AuditLogType
                ON AuditLogs (AuditLogType);
				CREATE INDEX idx_Audit_StartDate_AuditLogType
                ON AuditLogs (StartDate, AuditLogType);

				CREATE INDEX idx_Audit_ExceptionType
                ON AuditLogs (ExceptionType);
				CREATE INDEX idx_Audit_StartDate_ExceptionType
                ON AuditLogs (StartDate, ExceptionType);

				CREATE INDEX idx_Audit_HttpMethod
                ON AuditLogs (HttpMethod);
				CREATE INDEX idx_Audit_StartDate_HttpMethod
                ON AuditLogs (StartDate, HttpMethod);

				CREATE INDEX idx_Audit_HttpController
                ON AuditLogs (HttpController);
				CREATE INDEX idx_Audit_StartDate_HttpController
                ON AuditLogs (StartDate, HttpController);

				CREATE INDEX idx_Audit_HttpAction
                ON AuditLogs (HttpAction);
				CREATE INDEX idx_Audit_StartDate_HttpAction
                ON AuditLogs (StartDate, HttpAction);

				CREATE INDEX idx_Audit_HttpResultCode
                ON AuditLogs (HttpResultCode);
				CREATE INDEX idx_Audit_StartDate_HttpResultCode
                ON AuditLogs (StartDate, HttpResultCode);

				CREATE INDEX idx_Audit_HttpRequestIp
                ON AuditLogs (HttpRequestIp);
				CREATE INDEX idx_Audit_StartDate_HttpRequestIp
                ON AuditLogs (StartDate, HttpRequestIp);

				CREATE INDEX idx_Audit_UserId
                ON AuditLogs (UserId);
				CREATE INDEX idx_Audit_StartDate_UserId
                ON AuditLogs (StartDate, UserId);

				CREATE INDEX idx_Audit_UserEmail
                ON AuditLogs ([UserEmail]);
				CREATE INDEX idx_Audit_StartDate_UserEmail
                ON AuditLogs (StartDate, [UserEmail]);

            ";

            migrationBuilder.Sql(sqlScript);
        }

        private static StringBuilder GenerateSqlScriptForEachModelEntitieWithAttribute<T>(IModel targetModel, string sqlBaseStatement) where T : Attribute
        {
            StringBuilder finalSqlStatementStringBuilder = new StringBuilder();
            IEnumerable<IEntityType> modelEntitiesTypes = targetModel.GetEntityTypes();


            // Interates over every entity in the target model
            foreach (IEntityType entityType in modelEntitiesTypes)
            {
                // Checks if this Entity or some related entity (inheritance) is marked as Auditable
                if (Type.GetType(entityType.Name) is not null && Type.GetType(entityType.Name).HasAttribute<T>())
                {
                    // Gets the name of the PK property for this model entity
                    string _PK_Property = entityType.FindPrimaryKey().Properties
                                          .Select(x => x.Name).Single();

                    // Gets the real name for the table created in the database
                    string _Table_Name = entityType.GetTableName();
                    string _Schema_Name = entityType.GetSchema() ?? "dbo";

                    // Put the variables values in the sqlBaseStatement
                    string formattedSqlBaseStatement = string.Format(sqlBaseStatement, _Schema_Name, _Table_Name, _PK_Property);

                    finalSqlStatementStringBuilder.Append(formattedSqlBaseStatement);
                }
            }
            return finalSqlStatementStringBuilder;
        }
    }
}
