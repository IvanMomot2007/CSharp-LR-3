using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DurakCore.Patterns.Observer;
using DurakCore.Patterns.State;
using DurakCore.Patterns.Strategy;

namespace DurakCore.Models
{
    public class DurakGameModel
    {
        private readonly List<Player> _players = new();
        private readonly List<TablePair> _table = new();
        private readonly List<IGameObserver> _observers = new();
        private readonly HashSet<Player> _passedPlayersInRound = new();

        public IReadOnlyList<Player> Players => _players.AsReadOnly();
        public IReadOnlyList<Player> ActivePlayers => _players.Where(p => !p.Hand.IsEmpty || !Deck.IsEmpty).ToList().AsReadOnly();

        public Player Attacker { get; private set; } = null!;
        public Player Defender { get; private set; } = null!;
        public Player CurrentTurnPlayer { get; private set; } = null!;
        public Player? Loser { get; private set; }
        public bool IsDraw { get; private set; }

        public CardSet Deck { get; } = new();
        public CardSet DiscardPile { get; } = new();
        public IReadOnlyList<TablePair> Table => _table.AsReadOnly();

        public CardSuit TrumpSuit { get; private set; }
        public Card? TrumpCard { get; private set; }

        public IGameState CurrentState { get; private set; }
        public IBotStrategy BotStrategy { get; set; }

        public DurakGameModel(IEnumerable<string>? humanPlayerNames = null, IBotStrategy? botStrategy = null)
        {
            BotStrategy = botStrategy ?? new SmartBotStrategy();
            CurrentState = new GameOverState();
            ConfigurePlayers(humanPlayerNames ?? new[] { "Гравець" });
        }

        public void ConfigurePlayers(IEnumerable<string> humanPlayerNames)
        {
            if (humanPlayerNames == null)
                throw new ArgumentNullException(nameof(humanPlayerNames));

            List<string> names = humanPlayerNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .ToList();

            if (names.Count == 0 || names.Count > 4)
                throw new ArgumentException("Кількість гравців-людей має бути від 1 до 4.", nameof(humanPlayerNames));

            _players.Clear();
            foreach (string name in names)
            {
                _players.Add(new Player(name, isBot: false));
            }

            if (_players.Count == 1)
            {
                _players.Add(new Player("Бот", isBot: true));
            }

            Attacker = _players[0];
            Defender = _players[1];
            CurrentTurnPlayer = Attacker;
        }

        public void Attach(IGameObserver observer)
        {
            if (observer != null && !_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(IGameObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyStateChanged()
        {
            foreach (IGameObserver observer in _observers)
                observer.OnGameStateChanged(this);
        }

        public void NotifyMessage(string message)
        {
            foreach (IGameObserver observer in _observers)
                observer.OnGameMessage(message);
        }

        public void StartNewGame(Random? random = null)
        {
            foreach (Player player in _players)
                player.Hand.Clear();

            DiscardPile.Clear();
            _table.Clear();
            _passedPlayersInRound.Clear();
            Loser = null;
            IsDraw = false;

            Deck.FillStandard36();
            Deck.Shuffle(random);

            TrumpCard = Deck.PeekBottom();
            TrumpSuit = TrumpCard!.Suit;

            DealInitialCards();
            DetermineFirstAttacker();

            CurrentTurnPlayer = Attacker;
            SetState(new AttackState());
            NotifyMessage($"Нова гра ({_players.Count} гравців)! Козир: {TrumpSuit}. Першим ходить {Attacker.Name}.");
        }

        private void DealInitialCards()
        {
            for (int i = 0; i < 6; i++)
            {
                foreach (Player player in _players)
                {
                    if (!Deck.IsEmpty)
                        player.Hand.Add(Deck.DrawTop());
                }
            }
        }

        private void DetermineFirstAttacker()
        {
            Player firstAttacker = _players[0];
            CardRank lowestTrump = (CardRank)99;

            foreach (Player player in _players)
            {
                Card? minTrump = player.Hand.Cards
                    .Where(c => c.Suit == TrumpSuit)
                    .OrderBy(c => c.Rank)
                    .FirstOrDefault();

                if (minTrump != null && minTrump.Rank < lowestTrump)
                {
                    lowestTrump = minTrump.Rank;
                    firstAttacker = player;
                }
            }

            Attacker = firstAttacker;
            Defender = GetNextActivePlayer(Attacker);
        }

        public Player GetNextActivePlayer(Player fromPlayer)
        {
            int startIndex = _players.IndexOf(fromPlayer);
            for (int offset = 1; offset <= _players.Count; offset++)
            {
                Player candidate = _players[(startIndex + offset) % _players.Count];
                if (!candidate.Hand.IsEmpty || !Deck.IsEmpty)
                    return candidate;
            }
            return fromPlayer;
        }

        public bool PlayAttackCard(Card card) => CurrentState.Attack(this, card);
        public bool PlayDefendCard(Card card) => CurrentState.Defend(this, card);
        public bool Pass() => CurrentState.Pass(this);
        public bool TakeCards() => CurrentState.TakeCards(this);

        internal void SetState(IGameState newState)
        {
            CurrentState = newState;
            NotifyStateChanged();
        }

        internal void SetCurrentTurnPlayer(Player player)
        {
            CurrentTurnPlayer = player;
            NotifyStateChanged();
        }

        internal void AddTablePair(TablePair pair)
        {
            _table.Add(pair);
            _passedPlayersInRound.Clear(); 
        }

        internal bool RegisterPassAndCheckAllPassed()
        {
            _passedPlayersInRound.Add(CurrentTurnPlayer);

            int startIndex = _players.IndexOf(CurrentTurnPlayer);
            for (int offset = 1; offset < _players.Count; offset++)
            {
                Player candidate = _players[(startIndex + offset) % _players.Count];
                if (candidate == Defender)
                    continue;

                bool hasCards = !candidate.Hand.IsEmpty;
                if (hasCards && !_passedPlayersInRound.Contains(candidate))
                {
                    CurrentTurnPlayer = candidate;
                    NotifyMessage($"Черга підкидати переходить до {candidate.Name}.");
                    NotifyStateChanged();
                    return false;
                }
            }

            return true;
        }

        public TablePair? GetUnbeatenPair() => _table.FirstOrDefault(p => !p.IsBeaten);
        public bool HasUnbeatenCard() => _table.Any(p => !p.IsBeaten);

        public bool IsRankOnTable(CardRank rank)
        {
            return _table.Any(p => p.AttackCard.Rank == rank || (p.DefendCard != null && p.DefendCard.Rank == rank));
        }

        internal void FinishRound(bool defenderTookCards)
        {
            if (defenderTookCards)
            {
                foreach (TablePair pair in _table)
                {
                    Defender.Hand.Add(pair.AttackCard);
                    if (pair.DefendCard != null)
                        Defender.Hand.Add(pair.DefendCard);
                }
            }
            else
            {
                foreach (TablePair pair in _table)
                {
                    DiscardPile.Add(pair.AttackCard);
                    if (pair.DefendCard != null)
                        DiscardPile.Add(pair.DefendCard);
                }
            }

            _table.Clear();
            _passedPlayersInRound.Clear();
            RefillHands();

            if (CheckGameOver())
                return;

            if (defenderTookCards)
            {
                Attacker = GetNextActivePlayer(Defender);
            }
            else
            {
                Attacker = !Defender.Hand.IsEmpty ? Defender : GetNextActivePlayer(Defender);
            }

            Defender = GetNextActivePlayer(Attacker);
            CurrentTurnPlayer = Attacker;

            SetState(new AttackState());
        }

        private void RefillHands()
        {
            List<Player> refillOrder = new();
            int attackerIdx = _players.IndexOf(Attacker);

            for (int i = 0; i < _players.Count; i++)
            {
                Player p = _players[(attackerIdx + i) % _players.Count];
                if (p != Defender)
                    refillOrder.Add(p);
            }
            refillOrder.Add(Defender);

            foreach (Player player in refillOrder)
            {
                while (player.Hand.Count < 6 && !Deck.IsEmpty)
                {
                    player.Hand.Add(Deck.DrawTop());
                }
            }
        }

        internal bool CheckGameOver()
        {
            if (!Deck.IsEmpty)
                return false;

            List<Player> playersWithCards = _players.Where(p => !p.Hand.IsEmpty).ToList();

            if (playersWithCards.Count == 0)
            {
                IsDraw = true;
                Loser = null;
                NotifyMessage("Нічия! Усі гравці вийшли з гри одночасно.");
                SetState(new GameOverState());
                return true;
            }

            if (playersWithCards.Count == 1)
            {
                Loser = playersWithCards[0];
                NotifyMessage($"Гру завершено! У дурнях залишився {Loser.Name}.");
                SetState(new GameOverState());
                return true;
            }

            return false;
        }
    }
}