using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;

namespace DurakCore.Patterns.Strategy
{
    public interface IBotStrategy
    {
        Card? ChooseAttackCard(CardSet botHand, IReadOnlyList<TablePair> table, CardSuit trumpSuit);
        Card? ChooseDefendCard(CardSet botHand, Card attackCard, CardSuit trumpSuit);
    }
}
