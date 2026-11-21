using DurakCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Patterns.Strategy
{
    public class EasyBotStrategy : IBotStrategy
    {
        public Card? ChooseAttackCard(CardSet botHand, IReadOnlyList<TablePair> table, CardSuit trumpSuit)
        {
            if (botHand.IsEmpty) return null;

            if (table.Count == 0)
            {
                return botHand.Cards[0];
            }

            HashSet<CardRank> ranksOnTable = GetRanksOnTable(table);
            return botHand.Cards.FirstOrDefault(c => ranksOnTable.Contains(c.Rank));
        }

        public Card? ChooseDefendCard(CardSet botHand, Card attackCard, CardSuit trumpSuit)
        {
            return botHand.Cards.FirstOrDefault(c => DurakComparer.CanBeat(attackCard, c, trumpSuit));
        }

        private static HashSet<CardRank> GetRanksOnTable(IReadOnlyList<TablePair> table)
        {
            HashSet<CardRank> ranks = new HashSet<CardRank>();
            foreach (TablePair pair in table)
            {
                ranks.Add(pair.AttackCard.Rank);
                if (pair.DefendCard != null)
                    ranks.Add(pair.DefendCard.Rank);
            }
            return ranks;
        }
    }
}
