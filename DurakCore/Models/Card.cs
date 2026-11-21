using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Models
{
    public class Card
    {
        public CardSuit Suit { get; }
        public CardRank Rank { get; }

        public Card(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override bool Equals(object? obj)
        {
            return obj is Card other && Suit == other.Suit && Rank == other.Rank;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
