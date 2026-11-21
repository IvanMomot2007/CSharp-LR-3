using System;
using System.Collections.Generic;
using System.Text;

namespace DurakCore.Models
{
    public class Player
    {
        public string Name { get; }
        public bool IsBot { get; }
        public CardSet Hand { get; } = new();

        public Player(string name, bool isBot = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я гравця не може бути порожнім.", nameof(name));

            Name = name;
            IsBot = isBot;
        }

        public override string ToString() => $"{Name} (карт: {Hand.Count})";
    }
}
