using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Models
{
    public class CardSet
    {
        private readonly List<Card> _cards = new();

        public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public void Add(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            _cards.Add(card);
        }

        public void AddRange(IEnumerable<Card> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            foreach (Card card in cards)
            {
                Add(card);
            }
        }

        public bool Remove(Card card)
        {
            return _cards.Remove(card);
        }

        public bool Contains(Card card)
        {
            return _cards.Contains(card);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public void FillStandard36()
        {
            _cards.Clear();
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                {
                    _cards.Add(new Card(suit, rank));
                }
            }
        }

        public void Shuffle(Random? random = null)
        {
            random ??= Random.Shared;
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public Card DrawTop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Набір карт порожній.");

            Card top = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return top;
        }

        public Card? PeekBottom()
        {
            return IsEmpty ? null : _cards[0];
        }

        public bool HasRank(CardRank rank)
        {
            return _cards.Any(c => c.Rank == rank);
        }
    }
}
