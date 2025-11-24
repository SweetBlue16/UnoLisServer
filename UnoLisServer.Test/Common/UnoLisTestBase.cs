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
        protected string EntityConnectionString { get; private set; }

        protected UnoLisTestBase()
        {
            ConfigureConnection();
            _scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        }

        private void ConfigureConnection()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".", 
                InitialCatalog = "UNOLIS_TEST",
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework"
            };

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