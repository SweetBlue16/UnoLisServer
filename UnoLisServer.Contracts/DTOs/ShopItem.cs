using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class ShopItem
    {
        public int BoxId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
        public int Price { get; set; }
    }
}

