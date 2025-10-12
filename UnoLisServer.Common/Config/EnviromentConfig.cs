using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Config
{
    public static class EnvironmentConfig
    {
        public static string DbUser => Environment.GetEnvironmentVariable("UNO_DB_USER") ?? "unoUser";
        public static string DbPassword => Environment.GetEnvironmentVariable("UNO_DB_PASS") ?? "UNO2025!";
        public static string DbServer => Environment.GetEnvironmentVariable("UNO_DB_SERVER") ?? "EVICTUS";
        public static string DbName => Environment.GetEnvironmentVariable("UNO_DB_NAME") ?? "UNODataBase";

        public static string BuildConnectionString()
        {
            return $"Data Source={DbServer};Initial Catalog={DbName};User ID={DbUser};Password={DbPassword};TrustServerCertificate=True;MultipleActiveResultSets=True;";
        }
    }
}
