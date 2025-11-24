using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Transactions;
using UnoLisServer.Data;

namespace UnoLisServer.Test.Common
{
    public abstract class UnoLisTestBase : IDisposable
    {
        private readonly TransactionScope _scope;

        // Propiedad protegida para que los hijos la usen
        protected string EntityConnectionString { get; private set; }

        protected UnoLisTestBase()
        {
            ConfigureConnection();

            // Iniciamos la transacción automática
            _scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        }

        private void ConfigureConnection()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".", // Servidor Local
                InitialCatalog = "UNOLIS_TEST", // Base de Datos de Pruebas
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework"
            };

            // NOTA: Usamos el comodín 'res://*/' que le dice a EF: 
            // "Busca los metadatos en cualquier DLL cargada en memoria". 
            // Esto es ideal para pruebas unitarias.
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ToString(),
                Metadata = "res://*/UNODataBaseModel.csdl|res://*/UNODataBaseModel.ssdl|res://*/UNODataBaseModel.msl"
            };

            EntityConnectionString = entityBuilder.ToString();
        }

        protected UNOContext GetContext()
        {
            return new UNOContext(EntityConnectionString);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}