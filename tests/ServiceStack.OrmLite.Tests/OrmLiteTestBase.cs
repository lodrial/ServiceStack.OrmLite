using System;
using System.Data;
using System.IO;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class Config
    {
        public static string SqliteMemoryDb = ":memory:";
        public static string SqliteFileDir = "~/App_Data/".MapAbsolutePath();
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        public static string SqlServerDb = "~/App_Data/Database1.mdf".MapAbsolutePath();
        public static string SqlServerBuildDb = "Server={0};Database=test;User Id=test;Password=test;".Fmt(Environment.GetEnvironmentVariable("CI_HOST"));
        //public static string SqlServerBuildDb = "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=SSPI;Connect Timeout=120;MultipleActiveResultSets=True";

        public static IOrmLiteDialectProvider DefaultProvider = SqlServerDialect.Provider;
        public static string DefaultConnection = SqlServerBuildDb;

        public static string GetDefaultConnection()
        {
            OrmLiteConfig.DialectProvider = DefaultProvider;
            return DefaultConnection;
        }

        public static IDbConnection OpenDbConnection()
        {
            return GetDefaultConnection().OpenDbConnection();
        }
    }

	public class OrmLiteTestBase
	{
	    protected virtual string ConnectionString { get; set; }

		protected string GetConnectionString()
		{
			return GetFileConnectionString();
		}

	    public static OrmLiteConnectionFactory CreateSqlServerDbFactory()
	    {
            var dbFactory = new OrmLiteConnectionFactory(Config.SqlServerBuildDb, SqlServerDialect.Provider);
	        return dbFactory;
	    }

	    protected virtual string GetFileConnectionString()
		{
            var connectionString = Config.SqliteFileDb;
			if (File.Exists(connectionString))
				File.Delete(connectionString);

			return connectionString;
		}

		protected void CreateNewDatabase()
		{
			if (ConnectionString.Contains(".sqlite"))
				ConnectionString = GetFileConnectionString();
		}

        enum DbType
        {
            Sqlite,
            SqlServer,
            PostgreSql,
            MySql,
            SqlServerMdf,
        }

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

            var dbType = DbType.Sqlite;
            //var dbType = DbType.SqlServer;

		    switch (dbType)
		    {
		        case DbType.Sqlite:
                    OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
                    ConnectionString = GetFileConnectionString();
                    ConnectionString = ":memory:";
                    return;
                case DbType.SqlServer:
                    OrmLiteConfig.DialectProvider = SqlServerDialect.Provider;
                    ConnectionString = Config.SqlServerBuildDb;
                    return;
                case DbType.MySql:
                    OrmLiteConfig.DialectProvider = MySqlDialect.Provider;
                    ConnectionString = "Server=localhost;Database=test;UID=root;Password=test";
                    return;
                case DbType.PostgreSql:
                    OrmLiteConfig.DialectProvider = PostgreSqlDialect.Provider;
                    ConnectionString = Environment.GetEnvironmentVariable("PGSQL_TEST");
                    return;
                case DbType.SqlServerMdf:
                    ConnectionString = "~/App_Data/Database1.mdf".MapAbsolutePath();			
                    ConnectionString = Config.GetDefaultConnection();
                    return;
            }
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public IDbConnection InMemoryDbConnection { get; set; }

        public IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            if (connString == ":memory:")
            {
                if (InMemoryDbConnection == null)
                {
                    var dbConn = connString.OpenDbConnection();
                    InMemoryDbConnection = new OrmLiteConnectionWrapper(dbConn)
                    {
                        DialectProvider = OrmLiteConfig.DialectProvider,
                        AutoDisposeConnection = false,
                    };                    
                }

                return InMemoryDbConnection;
            }

            return connString.OpenDbConnection();            
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            // Not Oracle if this base class used
        }
	}
}