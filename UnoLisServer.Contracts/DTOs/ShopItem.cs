namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Item available in the shop
    /// </summary>
    public class ShopItem
    {
        public int BoxId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
        public int Price { get; set; }
    }
}

