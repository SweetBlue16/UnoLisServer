using System;
using System.Collections.Generic;
using System.Linq;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.GameLogic
{
    /// <summary>
    /// Manages the draw and discard piles for a deck of cards, providing operations to draw, discard, and recycle cards
    /// as needed.
    /// </summary>
    public class DeckManager
    {
        private readonly int emptyDeck = 0;
        private readonly int lastCard = 1;
        private Queue<Card> _drawPile;
        private readonly Stack<Card> _discardPile;

        public DeckManager()
        {
            var rawDeck = DeckFactory.CreateFullDeck();
            _drawPile = Shuffle(rawDeck.ToList());
            _discardPile = new Stack<Card>();
        }

        public Card DrawCard()
        {
            if (_drawPile.Count == emptyDeck)
            {
                RecycleDiscardPile();
            }

            if (_drawPile.Count == emptyDeck)
            {
                throw new InvalidOperationException("Deck and discard pile out.");
            }

            return _drawPile.Dequeue();
        }

        public List<Card> DrawCards(int count)
        {
            var cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                if (_drawPile.Count == emptyDeck)
                {
                    RecycleDiscardPile();
                }

                if (_drawPile.Count > emptyDeck)
                {
                    cards.Add(_drawPile.Dequeue());
                }
                else
                {
                    break;
                }
            }
            return cards;
        }

        public void AddToDiscardPile(Card card)
        {
            if (IsWildCard(card.Value))
            {
                card.Color = Contracts.Enums.CardColor.Wild;
            }

            _discardPile.Push(card);
        }


        /// <summary>
        /// Returning null is accepted here to handle throws
        /// </summary>
        /// <returns></returns>
        public Card PeekTopCard()
        {
            return _discardPile.Count > emptyDeck ? _discardPile.Peek() : null;
        }

        public Card StartDiscardPile()
        {
            var card = DrawCard();
            _discardPile.Push(card);
            return card;
        }

        private void RecycleDiscardPile()
        {
            
            if (_discardPile.Count <= lastCard)
            {
                return;
            }

            var topCard = _discardPile.Pop();
            var cardsToRecycle = _discardPile.ToList();
            _discardPile.Clear();
            _discardPile.Push(topCard);
            _drawPile = Shuffle(cardsToRecycle);

            Logger.Log("Deck recycled and reshuffled.");
        }

        private static Queue<Card> Shuffle(List<Card> cards)
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = SecureRandom.Next(0, n + 1);

                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
            return new Queue<Card>(cards);
        }

        private static bool IsWildCard(Contracts.Enums.CardValue value)
        {
            return value == Contracts.Enums.CardValue.Wild ||
                   value == Contracts.Enums.CardValue.WildDrawFour ||
                   value == Contracts.Enums.CardValue.WildDrawTen ||
                   value == Contracts.Enums.CardValue.WildDrawSkipReverseFour;
        }
    }
}