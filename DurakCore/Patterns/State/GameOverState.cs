using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;

namespace DurakCore.Patterns.State
{
    public class GameOverState : IGameState
    {
        public string StateName => "Гра завершена";

        public bool Attack(DurakGameModel game, Card card) => false;
        public bool Defend(DurakGameModel game, Card card) => false;
        public bool Pass(DurakGameModel game) => false;
        public bool TakeCards(DurakGameModel game) => false;
    }
}
