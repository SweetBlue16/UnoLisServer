using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.Enums;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class Card
    {
        [DataMember]
        public CardColor Color { get; set; }

        [DataMember]
        public CardValue Value { get; set; }

        [DataMember]
        public string ImagePath { get; set; } // ruta o nombre del asset (por ejemplo: "Assets/Cards/red_5.png")

        [DataMember]
        public string Description { get; set; } // opcional (ej: "Salta el turno del siguiente jugador")

        public Card() { }

        public Card(CardColor color, CardValue value, string imagePath = null, string description = null)
        {
            Color = color;
            Value = value;
            ImagePath = imagePath;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Color} {Value}";
        }
    }
}

