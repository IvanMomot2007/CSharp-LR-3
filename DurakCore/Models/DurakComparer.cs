using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Models
{
    public class DurakComparer
    {
        public static bool CanBeat(Card attackCard, Card defendCard, CardSuit trumpSuit)
        {
            if (attackCard == null) throw new ArgumentNullException(nameof(attackCard));
            if (defendCard == null) throw new ArgumentNullException(nameof(defendCard));

            if (attackCard.Suit == defendCard.Suit)
            {
                return defendCard.Rank > attackCard.Rank;
            }

            return defendCard.Suit == trumpSuit;
        }
    }
}
