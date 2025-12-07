using System;
using System.Data.SqlClient;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Common.Helpers
{
    public static class SqlErrorHandler
    {
        public static void HandleAndThrow(SqlException sqlEx)
        {
            string logMessage;
            bool isCritical = false;

            switch (sqlEx.Number)
            {
                case 2:
                case 53:
                    logMessage = $"[SQL-FATAL] SQL Server service appears to be DOWN, unreachable, or " +
                        $"network failure (Error {sqlEx.Number}).";
                    isCritical = true;
                    break;

                case 4060:
                    logMessage = $"[SQL-FATAL] The SQL Server is reachable, but the Database 'UnoLisDB' " +
                        $"does NOT exist or access was denied (Error {sqlEx.Number}).";
                    isCritical = true;
                    break;

                case 18456:
                    logMessage = $"[SQL-AUTH] Login failed for user (Invalid Username/Password in Connection String)" +
                        $" (Error {sqlEx.Number}).";
                    isCritical = true;
                    break;

                case -2:
                    logMessage = $"[SQL-TIMEOUT] Connection timeout expired. The server " +
                        $"might be overwhelmed or locked.";
                    break;

                case 547:
                    logMessage = $"[SQL-CONSTRAINT] Foreign Key violation (Error {sqlEx.Number}).";
                    break;
                case 2601:
                case 2627:
                    logMessage = $"[SQL-DUPLICATE] Unique constraint violation / Duplicate Key (Error {sqlEx.Number}).";
                    break;

                default:
                    logMessage = $"[SQL-ERROR] Generic Database Error: {sqlEx.Message} (Code {sqlEx.Number}).";
                    break;
            }


            if (isCritical)
            {
                Logger.Error(logMessage, sqlEx);
            }
            else
            {
                Logger.Error(logMessage, sqlEx);
            }

            throw new Exception("DataStore_Unavailable", sqlEx);
        }
    }
}
