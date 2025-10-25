using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Models
{
    public class CodeValidationRequest
    {
        public string Identifier { get; set; }
        public string Code { get; set; }
        public int CodeType { get; set; }
        public bool Consume { get; set; } = true;
    }
}
