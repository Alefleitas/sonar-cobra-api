using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace nordelta.cobra.webapi.tests
{
    static class TestDBHelper
    {

        public static SqliteConnection GetOpenedConnection()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        }

        public static RelationalDbContext GetPopulatedRelationalContext(SqliteConnection connection, string sqlDataScript)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<RelationalDbContext>()
                      .UseSqlite(connection)
                      .EnableSensitiveDataLogging()
                      .UseInternalServiceProvider(serviceProvider)
                      .Options;

            // Create the schema in the database
            var context = new RelationalDbContext(options);

            //Populates the db only if is just being created
            if (context.Database.EnsureCreated())
                PopulateDBWithTestData(context, sqlDataScript);

            return context;
        }

        public static InMemoryDbContext GetPopulatedInMemoryContext(SqliteConnection connection, string sqlDataScript)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<InMemoryDbContext>()
                    .UseSqlite(connection)
                    .EnableSensitiveDataLogging()
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;

            // Create the schema in the database
            var context = new InMemoryDbContext(options);

            //Populates the db only if is just being created
            if (context.Database.EnsureCreated())
                PopulateDBWithTestData(context, sqlDataScript);

            return context;
        }

        private static void PopulateDBWithTestData(RelationalDbContext context, string sqlDataScript)
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = File.ReadAllText(Path.Combine(projectDirectory, sqlDataScript));
                command.CommandType = CommandType.Text;

                command.ExecuteNonQuery();
            }
        }

        private static void PopulateDBWithTestData(InMemoryDbContext context, string sqlDataScript)
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = File.ReadAllText(Path.Combine(projectDirectory,sqlDataScript));
                command.CommandType = CommandType.Text;

                command.ExecuteNonQuery();
            }
        }

    }
}
