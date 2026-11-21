using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using DurakCore.Controllers;
using DurakCore.Models;
using DurakCore.Patterns.State;
using DurakCore.Patterns.Strategy;
using DurakCore.Views;

namespace DurakTests.IntegrationTests
{
    public class GameFlowIntegrationTests
    {
        private class MockGameView : IGameView
        {
            public int StateChangedCount { get; private set; }
            public List<string> Messages { get; } = new();
            public List<string> Errors { get; } = new();

            public void OnGameStateChanged(DurakGameModel game) => StateChangedCount++;
            public void OnGameMessage(string message) => Messages.Add(message);
            public void ShowError(string errorMessage) => Errors.Add(errorMessage);
        }

        [Fact]
        public void MultiplayerThreeHumans_PassAndPlayRound_RotatesTurnsProperly()
        {
            MockGameView view = new MockGameView();
            DurakGameModel model = new DurakGameModel(new[] { "Іван", "Влад", "Діма" });
            GameController controller = new GameController(model, view);

            controller.StartNewGame(new Random(10));

            Assert.Equal(3, model.Players.Count);
            Assert.False(controller.IsBotTurn());
            Assert.IsType<AttackState>(model.CurrentState);

            Player firstAttacker = model.Attacker;
            Player defender = model.Defender;
            Card attackCard = firstAttacker.Hand.Cards[0];

            Assert.True(controller.HumanPlayCard(attackCard));
            Assert.IsType<DefendState>(model.CurrentState);
            Assert.Equal(defender, model.CurrentTurnPlayer);

            Assert.True(controller.HumanTakeCards());
            Assert.IsType<AttackState>(model.CurrentState);
            Assert.Equal(model.GetNextActivePlayer(defender), model.Attacker);
        }

        [Fact]
        public void Controller_InvalidActions_TriggerViewErrors()
        {
            MockGameView view = new MockGameView();
            DurakGameModel model = new DurakGameModel(new[] { "Іван" }, new EasyBotStrategy());
            GameController controller = new GameController(model, view);

            Assert.False(controller.HumanPlayCard(new Card(CardSuit.Hearts, CardRank.Six)));
            Assert.False(controller.HumanPass());
            Assert.False(controller.HumanTakeCards());
            Assert.False(controller.IsBotTurn());
            Assert.False(controller.ExecuteBotStep());
            Assert.Equal(3, view.Errors.Count);

            controller.SetBotStrategy(new SmartBotStrategy());
            MockGameView view2 = new MockGameView();
            controller.AttachView(view2);
            controller.StartNewGame(new Random(7));
            Assert.True(view2.StateChangedCount > 0);
        }

        [Fact]
        public void AutomatedFullGame_FourPlayers_PlaysUntilOneLoserRemains()
        {
            DurakGameModel model = new DurakGameModel(new[] { "Гравець 1", "Гравець 2", "Гравець 3", "Гравець 4" });
            GameController controller = new GameController(model);
            SmartBotStrategy autoPlayer = new SmartBotStrategy();

            controller.StartNewGame(new Random(42));

            int maxSteps = 5000;
            int steps = 0;

            while (model.CurrentState is not GameOverState && steps < maxSteps)
            {
                steps++;
                Player active = model.CurrentTurnPlayer;

                if (model.CurrentState is AttackState)
                {
                    Card? card = autoPlayer.ChooseAttackCard(active.Hand, model.Table, model.TrumpSuit);
                    if (card == null || !controller.HumanPlayCard(card))
                    {
                        controller.HumanPass();
                    }
                }
                else if (model.CurrentState is DefendState)
                {
                    TablePair? unbeaten = model.GetUnbeatenPair();
                    Card? card = unbeaten != null
                        ? autoPlayer.ChooseDefendCard(active.Hand, unbeaten.AttackCard, model.TrumpSuit)
                        : null;

                    if (card == null || !controller.HumanPlayCard(card))
                    {
                        controller.HumanTakeCards();
                    }
                }
            }

            Assert.IsType<GameOverState>(model.CurrentState);
            Assert.True(model.Loser != null || model.IsDraw);
            Assert.True(model.Deck.IsEmpty);
        }
    }
}