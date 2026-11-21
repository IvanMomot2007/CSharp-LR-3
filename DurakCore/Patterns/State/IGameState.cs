using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;

namespace DurakCore.Patterns.State
{
    public interface IGameState
    {
        string StateName { get; }
        bool Attack(DurakGameModel game, Card card);
        bool Defend(DurakGameModel game, Card card);
        bool Pass(DurakGameModel game);
        bool TakeCards(DurakGameModel game);
    }
}
