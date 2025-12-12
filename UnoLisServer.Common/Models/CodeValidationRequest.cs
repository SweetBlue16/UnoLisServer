namespace UnoLisServer.Common.Models
{
    /// <summary>
    /// Validation request for codes such as verification or password reset codes.
    /// </summary>
    public class CodeValidationRequest
    {
        public string Identifier { get; set; }
        public string Code { get; set; }
        public int CodeType { get; set; }
        public bool Consume { get; set; } = true;
    }
}
