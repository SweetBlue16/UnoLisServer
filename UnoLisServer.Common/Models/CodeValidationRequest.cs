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
