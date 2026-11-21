using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;

namespace DurakCore.Patterns.State
{
    public class AttackState : IGameState
    {
        public string StateName => "Атака";

        public bool Attack(DurakGameModel game, Card card)
        {
            Player attacker = game.CurrentTurnPlayer;

            if (card == null || attacker == game.Defender || !attacker.Hand.Contains(card))
                return false;

            if (game.Table.Count >= 6 || game.Defender.Hand.IsEmpty)
                return false;

            if (game.Table.Count > 0 && !game.IsRankOnTable(card.Rank))
                return false;

            attacker.Hand.Remove(card);
            game.AddTablePair(new TablePair(card));
            game.NotifyMessage($"{attacker.Name} ходить картою {card}.");

            game.SetCurrentTurnPlayer(game.Defender);
            game.SetState(new DefendState());
            return true;
        }

        public bool Defend(DurakGameModel game, Card card) => false;

        public bool Pass(DurakGameModel game)
        {
            if (game.Table.Count == 0 || game.HasUnbeatenCard())
                return false;

            game.NotifyMessage($"{game.CurrentTurnPlayer.Name} каже «Пас / Бито».");

            if (game.RegisterPassAndCheckAllPassed())
            {
                game.NotifyMessage("Усі гравці спасували — «Бито»!");
                game.FinishRound(defenderTookCards: false);
            }

            return true;
        }

        public bool TakeCards(DurakGameModel game) => false;
    }
}
