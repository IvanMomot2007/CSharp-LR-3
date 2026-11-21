using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using DurakCore.Models;
using DurakCore.Patterns.Singleton;
using DurakCore.Patterns.State;
using DurakCore.Patterns.Strategy;

namespace DurakTests.UnitTests
{
    public class PatternsUnitTests
    {
        [Fact]
        public void Singleton_GameEngine_ConfiguresSingleAndMultiplayerModes()
        {
            GameEngine e1 = GameEngine.Instance;
            GameEngine e2 = GameEngine.Instance;

            Assert.Same(e1, e2);

            e1.Initialize(new[] { "Іван" }, new EasyBotStrategy());
            Assert.Equal(2, e2.Model.Players.Count);
            Assert.Equal("Іван", e2.Model.Players[0].Name);
            Assert.True(e2.Model.Players[1].IsBot);

            e1.Initialize(new[] { "Іван", "Влад", "Діма" });
            Assert.Equal(3, e2.Model.Players.Count);
            Assert.All(e2.Model.Players, p => Assert.False(p.IsBot));
        }

        [Fact]
        public void EasyBotStrategy_ChoosesFirstValidCards()
        {
            EasyBotStrategy strategy = new EasyBotStrategy();
            CardSet hand = new CardSet();
            List<TablePair> table = new List<TablePair>();

            Assert.Null(strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            Card c1 = new Card(CardSuit.Hearts, CardRank.Six);
            Card c2 = new Card(CardSuit.Clubs, CardRank.Ten);
            hand.AddRange(new[] { c1, c2 });

            Assert.Equal(c1, strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            TablePair pair = new TablePair(new Card(CardSuit.Diamonds, CardRank.Ten));
            pair.BeatWith(new Card(CardSuit.Diamonds, CardRank.Jack));
            table.Add(pair);
            Assert.Equal(c2, strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            Card? def = strategy.ChooseDefendCard(hand, new Card(CardSuit.Clubs, CardRank.Eight), CardSuit.Spades);
            Assert.Equal(c2, def);
        }

        [Fact]
        public void SmartBotStrategy_SavesTrumpsAndUsesMinimalCards()
        {
            SmartBotStrategy strategy = new SmartBotStrategy();
            CardSet hand = new CardSet();
            List<TablePair> table = new List<TablePair>();

            Assert.Null(strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            Card lowTrump = new Card(CardSuit.Spades, CardRank.Six);
            Card highNonTrump = new Card(CardSuit.Hearts, CardRank.King);
            Card lowNonTrump = new Card(CardSuit.Clubs, CardRank.Seven);
            hand.AddRange(new[] { lowTrump, highNonTrump, lowNonTrump });

            Assert.Equal(lowNonTrump, strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            table.Add(new TablePair(new Card(CardSuit.Diamonds, CardRank.Six)));
            Assert.Null(strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            table.Add(new TablePair(new Card(CardSuit.Diamonds, CardRank.Seven)));
            Assert.Equal(lowNonTrump, strategy.ChooseAttackCard(hand, table, CardSuit.Spades));

            Card? def = strategy.ChooseDefendCard(hand, new Card(CardSuit.Hearts, CardRank.Six), CardSuit.Spades);
            Assert.Equal(highNonTrump, def);
        }

        [Fact]
        public void GameOverState_BlocksAllActions()
        {
            DurakGameModel model = new DurakGameModel();
            GameOverState state = new GameOverState();
            Card card = new Card(CardSuit.Hearts, CardRank.Six);

            Assert.Equal("Гра завершена", state.StateName);
            Assert.False(state.Attack(model, card));
            Assert.False(state.Defend(model, card));
            Assert.False(state.Pass(model));
            Assert.False(state.TakeCards(model));
        }
    }
}
