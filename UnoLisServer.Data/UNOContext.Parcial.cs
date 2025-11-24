using System.Data.Common;
using System.Data.Entity;

namespace UnoLisServer.Data
{
    public partial class UNOContext : DbContext
    {
        public UNOContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public UNOContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }
    }
}