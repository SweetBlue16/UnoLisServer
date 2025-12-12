using System;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace UnoLisServer.Host
{
    internal static class ConfigInitializer
    {
        /// <summary>
        /// Builds and applies connection string to the application configuration
        /// </summary>
        public static void ApplyDatabaseConnectionFromEnv()
        {
            string server = Environment.GetEnvironmentVariable("UNO_DB_SERVER") ?? "localhost";
            string user = Environment.GetEnvironmentVariable("UNO_DB_USER") ?? "unoUser";
            string password = Environment.GetEnvironmentVariable("UNO_DB_PASSWORD") ?? "changeme";

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = "UNODataBase",
                UserID = user,
                Password = password,
                MultipleActiveResultSets = true,
                TrustServerCertificate = true
            };

            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ToString(),
                Metadata = "res://*/UNODataBaseModel.csdl|res://*/UNODataBaseModel.ssdl|res://*/UNODataBaseModel.msl"
            };

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.ConnectionStrings.ConnectionStrings["UNOContext"] != null)
            {
                config.ConnectionStrings.ConnectionStrings["UNOContext"].ConnectionString = entityBuilder.ToString();
            }
            else
            {
                config.ConnectionStrings.ConnectionStrings.Add(
                    new ConnectionStringSettings("UNOContext", entityBuilder.ToString(), "System.Data.EntityClient"));
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"✅ Conexión configurada para servidor: {server}");
            Console.ResetColor();
        }
    }
}
