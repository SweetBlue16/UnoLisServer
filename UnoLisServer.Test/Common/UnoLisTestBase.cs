using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Reflection;
using System.Transactions;
using UnoLisServer.Data;

namespace UnoLisServer.Test.Common
{
    public abstract class UnoLisTestBase : IDisposable
    {
        private readonly TransactionScope _scope;
        protected SqlConnection SharedSqlConnection { get; private set; }

        protected UnoLisTestBase()
        {
            // CONFIGURACIÓN CRÍTICA PARA EVITAR DEADLOCKS:
            // Usamos IsolationLevel.ReadCommitted. 
            // El default (Serializable) es demasiado agresivo bloqueando tablas enteras.
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30)
            };

            // 1. PRIMERO: Creamos el Scope con las opciones relajadas
            _scope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            // 2. SEGUNDO: Configuramos y Abrimos la conexión
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "UNOLIS_TEST",
                IntegratedSecurity = true,
                MultipleActiveResultSets = true, // Necesario para EF
                ApplicationName = "EntityFramework_Test"
            };

            SharedSqlConnection = new SqlConnection(sqlBuilder.ToString());
            SharedSqlConnection.Open();
        }

        protected UNOContext GetContext()
        {
            string[] metadataPaths =
            {
                "res://*/UNODataBaseModel.csdl",
                "res://*/UNODataBaseModel.ssdl",
                "res://*/UNODataBaseModel.msl"
            };

            Assembly dataAssembly = typeof(UNOContext).Assembly;
            MetadataWorkspace workspace = new MetadataWorkspace(metadataPaths, new[] { dataAssembly });
            EntityConnection entityConnection = new EntityConnection(workspace, SharedSqlConnection);

            return new UNOContext(entityConnection, false);
        }

        public void Dispose()
        {
            SharedSqlConnection?.Dispose();
            _scope?.Dispose(); // Rollback automático
        }
    }
}