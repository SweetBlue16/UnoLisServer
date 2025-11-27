using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Test.Common
{
    public static class SqlExceptionBuilder
    {
        public static SqlException Build()
        {
            return FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;
        }
    }
}
