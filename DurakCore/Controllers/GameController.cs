using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;
using DurakCore.Patterns.State;
using DurakCore.Patterns.Strategy;
using DurakCore.Views;

namespace DurakCore.Controllers
{
    public class GameController
    {
        public DurakGameModel Model { get; }
        private IGameView? _view;

        public GameController(DurakGameModel model, IGameView? view = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            if (view != null)
            {
                AttachView(view);
            }
        }

        public void AttachView(IGameView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (_view != null)
            {
                Model.Detach(_view);
            }

            _view = view;
            Model.Attach(_view);
        }

        public void SetupPlayers(IEnumerable<string> humanPlayerNames)
        {
            Model.ConfigurePlayers(humanPlayerNames);
        }

        public void SetBotStrategy(IBotStrategy strategy)
        {
            Model.BotStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            Model.NotifyMessage($"Стратегію бота змінено на {strategy.GetType().Name}.");
        }

        public void StartNewGame(Random? random = null)
        {
            Model.StartNewGame(random);
        }


        public bool HumanPlayCard(Card card)
        {
            if (Model.CurrentTurnPlayer.IsBot)
            {
                _view?.ShowError("Зараз хід бота.");
                return false;
            }

            if (Model.CurrentState is AttackState)
            {
                bool success = Model.PlayAttackCard(card);
                if (!success) _view?.ShowError("Неможливо походити або підкинути цю карту.");
                return success;
            }

            if (Model.CurrentState is DefendState)
            {
                bool success = Model.PlayDefendCard(card);
                if (!success) _view?.ShowError("Ця карта не може побити карту атаки.");
                return success;
            }

            _view?.ShowError("Гра вже завершена.");
            return false;
        }

        public bool HumanPass()
        {
            if (Model.CurrentTurnPlayer.IsBot || Model.CurrentState is not AttackState)
            {
                _view?.ShowError("Зараз неможливо оголосити «Бито».");
                return false;
            }

            bool success = Model.Pass();
            if (!success) _view?.ShowError("Не можна сказати «Бито» на порожньому столі або при непобитій карті.");
            return success;
        }

        public bool HumanTakeCards()
        {
            if (Model.CurrentTurnPlayer.IsBot || Model.CurrentState is not DefendState)
            {
                _view?.ShowError("Зараз не фаза захисту.");
                return false;
            }

            bool success = Model.TakeCards();
            if (!success) _view?.ShowError("На столі немає карт для взяття.");
            return success;
        }


        public bool IsBotTurn()
        {
            return Model.CurrentState is not GameOverState && Model.CurrentTurnPlayer.IsBot;
        }

        public bool ExecuteBotStep()
        {
            if (!IsBotTurn())
                return false;

            Player bot = Model.CurrentTurnPlayer;

            if (Model.CurrentState is AttackState)
            {
                Card? attackCard = Model.BotStrategy.ChooseAttackCard(bot.Hand, Model.Table, Model.TrumpSuit);
                if (attackCard != null && Model.PlayAttackCard(attackCard))
                    return true;

                return Model.Pass();
            }

            if (Model.CurrentState is DefendState)
            {
                TablePair? unbeaten = Model.GetUnbeatenPair();
                if (unbeaten == null) return false;

                Card? defendCard = Model.BotStrategy.ChooseDefendCard(bot.Hand, unbeaten.AttackCard, Model.TrumpSuit);
                if (defendCard != null && Model.PlayDefendCard(defendCard))
                    return true;

                return Model.TakeCards();
            }

            return false;
        }
    }
}
