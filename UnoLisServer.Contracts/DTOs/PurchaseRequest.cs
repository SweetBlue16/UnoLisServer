namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Tempting to purchase an item
    /// </summary>
    public class PurchaseRequest
    {
        public string Nickname { get; set; }
        public int ItemId { get; set; }
    }
}
