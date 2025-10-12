using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static OperationResult Ok(string msg = "Operación exitosa") =>
            new OperationResult { Success = true, Message = msg };

        public static OperationResult Fail(string msg = "Error en la operación") =>
            new OperationResult { Success = false, Message = msg };
    }
}
