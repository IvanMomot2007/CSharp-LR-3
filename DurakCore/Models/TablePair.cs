using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Models
{
    public class TablePair
    {
        public Card AttackCard { get; }
        public Card? DefendCard { get; private set; }
        public bool IsBeaten => DefendCard != null;

        public TablePair(Card attackCard)
        {
            AttackCard = attackCard ?? throw new ArgumentNullException(nameof(attackCard));
        }

        public void BeatWith(Card defendCard)
        {
            DefendCard = defendCard ?? throw new ArgumentNullException(nameof(defendCard));
        }

        public override string ToString()
        {
            return IsBeaten ? $"{AttackCard} <- {DefendCard}" : $"{AttackCard} (не побита)";
        }
    }
}
