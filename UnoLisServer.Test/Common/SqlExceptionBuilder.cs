using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Test.Common
{
    public static class SqlExceptionBuilder
    {
        public static SqlException Build()
        {
            var exception = FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;

            var errors = FormatterServices.GetUninitializedObject(typeof(SqlErrorCollection)) as SqlErrorCollection;

            var error = FormatterServices.GetUninitializedObject(typeof(SqlError)) as SqlError;

            SetPrivateField(error, "number", 1234);
            SetPrivateField(error, "message", "Fake SQL Error for Testing");

            var errorsListField = typeof(SqlErrorCollection).GetField("errors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (errorsListField != null)
            {
                var list = (ArrayList)errorsListField.GetValue(errors);
                if (list == null)
                {
                    list = new ArrayList();
                    errorsListField.SetValue(errors, list);
                }
                list.Add(error);
            }

            SetPrivateField(exception, "_errors", errors);

            return exception;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
