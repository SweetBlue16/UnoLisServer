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
        public string Id { get; set; }

        [DataMember]
        public CardColor Color { get; set; }

        [DataMember]
        public CardValue Value { get; set; }

        public override string ToString()
        {
            return $"{Color} {Value}";
        }
    }
}

