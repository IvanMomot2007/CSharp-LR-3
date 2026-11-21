using DurakCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Patterns.Strategy
{
    public class SmartBotStrategy : IBotStrategy
    {
        public Card? ChooseAttackCard(CardSet botHand, IReadOnlyList<TablePair> table, CardSuit trumpSuit)
        {
            if (botHand.IsEmpty) return null;

            IEnumerable<Card> candidates;

            if (table.Count == 0)
            {
                candidates = botHand.Cards;
            }
            else
            {
                HashSet<CardRank> ranksOnTable = GetRanksOnTable(table);
                candidates = botHand.Cards.Where(c => ranksOnTable.Contains(c.Rank));

                List<Card> nonTrumpCandidates = candidates.Where(c => c.Suit != trumpSuit).ToList();
                if (nonTrumpCandidates.Count > 0)
                {
                    candidates = nonTrumpCandidates;
                }
                else
                {
                    return null;
                }
            }

            return candidates
                .OrderBy(c => c.Suit == trumpSuit ? 1 : 0)
                .ThenBy(c => c.Rank)
                .FirstOrDefault();
        }

        public Card? ChooseDefendCard(CardSet botHand, Card attackCard, CardSuit trumpSuit)
        {
            List<Card> validCards = botHand.Cards
                .Where(c => DurakComparer.CanBeat(attackCard, c, trumpSuit))
                .OrderBy(c => c.Suit == trumpSuit ? 1 : 0)
                .ThenBy(c => c.Rank)
                .ToList();

            return validCards.FirstOrDefault();
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
