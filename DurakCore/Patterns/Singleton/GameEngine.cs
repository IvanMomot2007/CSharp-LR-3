using System;
using System.Collections.Generic;
using System.Text;
using DurakCore.Controllers;
using DurakCore.Models;
using DurakCore.Patterns.Strategy;
using DurakCore.Views;

namespace DurakCore.Patterns.Singleton
{
    public sealed class GameEngine
    {
        private static readonly Lazy<GameEngine> _instance = new(() => new GameEngine());

        public static GameEngine Instance => _instance.Value;

        public DurakGameModel Model { get; private set; }
        public GameController Controller { get; private set; }

        private GameEngine()
        {
            Model = new DurakGameModel();
            Controller = new GameController(Model);
        }

        public void Initialize(IEnumerable<string> humanPlayerNames, IBotStrategy? strategy = null, IGameView? view = null)
        {
            Model = new DurakGameModel(humanPlayerNames, strategy);
            Controller = new GameController(Model, view);
        }
    }
}