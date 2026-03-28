using Microsoft.Data.SqlClient;

namespace PR.Persistence.EntityFrameworkCore.SqlServer
{
    public static class ConnectionStringProvider
    {
        private static string _connectionString;

        static ConnectionStringProvider()
        {
            // Todo: Dette skal med igen, så man kan læse connection string fra en konfigurationsfil
            //var configFile = ConfigurationManager<>.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var settings = configFile.AppSettings.Settings;
            //var host = settings["Host"]?.Value;
            //var initialCatalog = settings["InitialCatalog"]?.Value;
            //var userID = settings["UserID"]?.Value;
            //var password = settings["Password"]?.Value;

            //if (host != null &&
            //    initialCatalog != null &&
            //    userID != null &&
            //    password != null)
            //{
            //    Initialize(host, initialCatalog, userID, password);
            //}
        }

        public static void Initialize(
            string host,
            string initialCatalog,
            string userID,
            string password)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = host,
                InitialCatalog = initialCatalog,
                UserID = userID,
                Password = password,
                TrustServerCertificate = true
            };

            _connectionString = sqlConnectionStringBuilder.ToString();
        }

        public static string GetConnectionString()
        {
            if (_connectionString == null)
            {
                // If we are here, it may be because we are enabling migrations with the Package Manager Console,
                // Then, we should just return a connection string for the repository on MELO-HOME, where we are
                // developing the solution.
                // Todo: read it from a file instead - it shouldn't be part of source code

                var defaultConnStringBuilder = new SqlConnectionStringBuilder
                {
                    UserID = "sa",
                    Password = "L1on8Zebra",
                    InitialCatalog = "PR_Trimmed",
                    DataSource = "melo-home\\sqlexpress",
                    TrustServerCertificate = true
                };

                return defaultConnStringBuilder.ToString();
            }

            return _connectionString;
        }
    }
}
