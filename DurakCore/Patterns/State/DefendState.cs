using DurakCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Patterns.State
{
    public class DefendState : IGameState
    {
        public string StateName => "Захист";

        public bool Attack(DurakGameModel game, Card card) => false;

        public bool Defend(DurakGameModel game, Card card)
        {
            if (card == null || !game.Defender.Hand.Contains(card))
                return false;

            TablePair? unbeatenPair = game.GetUnbeatenPair();
            if (unbeatenPair == null)
                return false;

            if (!DurakComparer.CanBeat(unbeatenPair.AttackCard, card, game.TrumpSuit))
                return false;

            game.Defender.Hand.Remove(card);
            unbeatenPair.BeatWith(card);
            game.NotifyMessage($"{game.Defender.Name} відбивається картою {card}.");

            if (game.CheckGameOver())
                return true;

            if (game.Defender.Hand.IsEmpty || game.Table.Count >= 6)
            {
                game.FinishRound(defenderTookCards: false);
            }
            else
            {
                game.SetCurrentTurnPlayer(game.Attacker);
                game.SetState(new AttackState());
            }

            return true;
        }

        public bool Pass(DurakGameModel game) => false;

        public bool TakeCards(DurakGameModel game)
        {
            if (game.Table.Count == 0)
                return false;

            game.NotifyMessage($"{game.Defender.Name} забирає карти зі столу.");
            game.FinishRound(defenderTookCards: true);
            return true;
        }
    }
}
