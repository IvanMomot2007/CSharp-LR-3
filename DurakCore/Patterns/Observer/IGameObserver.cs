using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Models;

namespace DurakCore.Patterns.Observer
{
    public interface IGameObserver
    {
        void OnGameStateChanged(DurakGameModel game);
        void OnGameMessage(string message);
    }
}
