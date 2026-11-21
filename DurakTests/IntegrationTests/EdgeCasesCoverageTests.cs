using System;
using System.Collections.Generic;
using Xunit;
using DurakCore.Controllers;
using DurakCore.Models;
using DurakCore.Patterns.State;
using DurakCore.Patterns.Strategy;
using DurakCore.Views;

namespace DurakTests.IntegrationTests
{
    public class EdgeCasesCoverageTests
    {
        private class DummyView : IGameView
        {
            public int Updates { get; private set; }
            public int Errors { get; private set; }
            public void OnGameStateChanged(DurakGameModel game) => Updates++;
            public void OnGameMessage(string message) { }
            public void ShowError(string errorMessage) => Errors++;
        }

        [Fact]
        public void BotVsHuman_FullAutomatedGame_CoversBotStepsAndController()
        {
            DummyView view = new DummyView();
            DurakGameModel model = new DurakGameModel(new[] { "Іван" }, new EasyBotStrategy());
            GameController controller = new GameController(model, view);

            controller.SetupPlayers(new[] { "Іван" });
            model.Detach(view);
            model.Attach(view);

            controller.StartNewGame(new Random(99));
            SmartBotStrategy humanHelper = new SmartBotStrategy();

            int steps = 0;
            while (model.CurrentState is not GameOverState && steps < 3000)
            {
                steps++;
                if (controller.IsBotTurn())
                {
                    Assert.True(controller.ExecuteBotStep());
                }
                else if (model.CurrentState is AttackState)
                {
                    Card? c = humanHelper.ChooseAttackCard(model.Players[0].Hand, model.Table, model.TrumpSuit);
                    if (c == null || !controller.HumanPlayCard(c))
                        controller.HumanPass();
                }
                else if (model.CurrentState is DefendState)
                {
                    TablePair? unbeaten = model.GetUnbeatenPair();
                    Card? c = unbeaten != null ? humanHelper.ChooseDefendCard(model.Players[0].Hand, unbeaten.AttackCard, model.TrumpSuit) : null;

                    if (c == null || !controller.HumanPlayCard(c))
                        controller.HumanTakeCards();
                }
            }

            Assert.IsType<GameOverState>(model.CurrentState);
            Assert.True(view.Updates > 0);
        }

        [Fact]
        public void ModelAndStates_ValidationAndInvalidTransitions_CoverBranches()
        {
            DurakGameModel model = new DurakGameModel();
            Assert.Throws<ArgumentNullException>(() => model.ConfigurePlayers(null!));
            Assert.Throws<ArgumentException>(() => model.ConfigurePlayers(Array.Empty<string>()));
            Assert.Throws<ArgumentException>(() => model.ConfigurePlayers(new[] { "1", "2", "3", "4", "5" }));

            Assert.Throws<ArgumentNullException>(() => new GameController(null!));
            GameController controller = new GameController(model);
            Assert.Throws<ArgumentNullException>(() => controller.AttachView(null!));
            Assert.Throws<ArgumentNullException>(() => controller.SetBotStrategy(null!));

            controller.StartNewGame(new Random(5));
            Assert.IsType<AttackState>(model.CurrentState);
            Assert.False(model.PlayDefendCard(new Card(CardSuit.Hearts, CardRank.Six)));
            Assert.False(model.TakeCards());
            Assert.False(model.PlayAttackCard(null!));

            Card validAttack = model.CurrentTurnPlayer.Hand.Cards[0];
            Assert.True(model.PlayAttackCard(validAttack));
            Assert.IsType<DefendState>(model.CurrentState);

            Assert.False(model.PlayAttackCard(validAttack));
            Assert.False(model.Pass());
            Assert.False(model.PlayDefendCard(null!));
        }

        [Fact]
        public void DrawCondition_WhenBothPlayersEmptyHandsSimultaneously_SetsIsDraw()
        {
            DurakGameModel model = new DurakGameModel(new[] { "Гравець 1", "Гравець 2" });
            model.StartNewGame(new Random(1));

            while (!model.Deck.IsEmpty)
                model.Deck.DrawTop();

            model.Attacker.Hand.Clear();
            model.Defender.Hand.Clear();

            Card attackCard = new Card(CardSuit.Hearts, CardRank.Six);
            Card defendCard = new Card(CardSuit.Hearts, CardRank.Seven);
            model.Attacker.Hand.Add(attackCard);
            model.Defender.Hand.Add(defendCard);

            Assert.True(model.PlayAttackCard(attackCard));
            Assert.True(model.PlayDefendCard(defendCard));

            Assert.IsType<GameOverState>(model.CurrentState);
            Assert.True(model.IsDraw);
            Assert.Null(model.Loser);
        }
    }
}