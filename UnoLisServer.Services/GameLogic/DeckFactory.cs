using System;
using System.Collections.Generic;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Enums;

namespace UnoLisServer.Services.GameLogic
{
    public static class DeckFactory
    {
        public static Queue<Card> CreateFullDeck()
        {
            var cards = new List<Card>();
            var colors = new[] { CardColor.Red, CardColor.Blue, CardColor.Green, CardColor.Yellow };

            foreach (var color in colors)
            {
                cards.Add(CreateCard(color, CardValue.Zero));

                for (int i = 0; i < 2; i++)
                {
                    cards.Add(CreateCard(color, CardValue.One));
                    cards.Add(CreateCard(color, CardValue.Two));
                    cards.Add(CreateCard(color, CardValue.Three));
                    cards.Add(CreateCard(color, CardValue.Four));
                    cards.Add(CreateCard(color, CardValue.Five));
                    cards.Add(CreateCard(color, CardValue.Six));
                    cards.Add(CreateCard(color, CardValue.Seven));
                    cards.Add(CreateCard(color, CardValue.Eight));
                    cards.Add(CreateCard(color, CardValue.Nine));
                }

                for (int i = 0; i < 2; i++)
                {
                    cards.Add(CreateCard(color, CardValue.Skip));
                    cards.Add(CreateCard(color, CardValue.Reverse));
                    cards.Add(CreateCard(color, CardValue.DrawTwo));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                cards.Add(CreateCard(CardColor.Wild, CardValue.Wild));
            }

            for (int i = 0; i < 4; i++)
            {
                cards.Add(CreateCard(CardColor.Wild, CardValue.WildDrawFour));
            }

            for (int i = 0; i < 2; i++)
            {
                cards.Add(CreateCard(CardColor.Wild, CardValue.WildDrawTen));
            }

            for (int i = 0; i < 2; i++)
            {
                cards.Add(CreateCard(CardColor.Wild, CardValue.WildDrawSkipReverseFour));
            }

            return new Queue<Card>(cards);
        }

        private static Card CreateCard(CardColor color, CardValue value)
        {
            return new Card
            {
                Id = Guid.NewGuid().ToString(),
                Color = color,
                Value = value
            };
        }
    }
}