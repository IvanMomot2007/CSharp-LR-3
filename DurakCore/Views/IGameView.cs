using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Patterns.Observer;

namespace DurakCore.Views
{
    public interface IGameView : IGameObserver
    {
        void ShowError(string errorMessage);
    }
}
