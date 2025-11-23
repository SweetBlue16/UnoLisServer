using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.Entity;

namespace UnoLisServer.Data
{
    public partial class UNOContext : DbContext
    {
        public UNOContext(DbConnection connection) : base(connection, true)
        {
        }
    }
}