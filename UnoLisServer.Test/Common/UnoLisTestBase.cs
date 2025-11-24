using System;
using System.Collections.Generic;
using System.Data.Common;
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
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "UNOLIS_TEST",
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework"
            };

            SharedSqlConnection = new SqlConnection(sqlBuilder.ToString());
            SharedSqlConnection.Open();

            _scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
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
            _scope.Dispose();          
            SharedSqlConnection.Dispose();
        }
    }
}