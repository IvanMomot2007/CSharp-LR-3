using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using DurakCore.Models;

namespace DurakTests.UnitTests
{
    public class CoreModelsUnitTests
    {
        [Fact]
        public void Card_EqualityHashCodeAndToString_WorkCorrectly()
        {
            Card c1 = new Card(CardSuit.Hearts, CardRank.Ace);
            Card c2 = new Card(CardSuit.Hearts, CardRank.Ace);
            Card c3 = new Card(CardSuit.Spades, CardRank.Six);

            Assert.Equal(c1, c2);
            Assert.Equal(c1.GetHashCode(), c2.GetHashCode());
            Assert.NotEqual(c1, c3);
            Assert.False(c1.Equals("not a card"));
            Assert.Equal("Ace of Hearts", c1.ToString());
        }

        [Fact]
        public void DurakComparer_CanBeat_ValidatesAllRules()
        {
            Card attack = new Card(CardSuit.Hearts, CardRank.Ten);

            Assert.True(DurakComparer.CanBeat(attack, new Card(CardSuit.Hearts, CardRank.Jack), CardSuit.Spades));
            Assert.False(DurakComparer.CanBeat(attack, new Card(CardSuit.Hearts, CardRank.Nine), CardSuit.Spades));
            Assert.False(DurakComparer.CanBeat(attack, new Card(CardSuit.Clubs, CardRank.Ace), CardSuit.Spades));
            Assert.True(DurakComparer.CanBeat(attack, new Card(CardSuit.Spades, CardRank.Six), CardSuit.Spades));
            Assert.False(DurakComparer.CanBeat(
                new Card(CardSuit.Spades, CardRank.King),
                new Card(CardSuit.Spades, CardRank.Queen),
                CardSuit.Spades));

            Assert.Throws<ArgumentNullException>(() => DurakComparer.CanBeat(null!, attack, CardSuit.Spades));
            Assert.Throws<ArgumentNullException>(() => DurakComparer.CanBeat(attack, null!, CardSuit.Spades));
        }

        [Fact]
        public void CardSet_OperationsAndExceptions_WorkCorrectly()
        {
            CardSet set = new CardSet();
            Assert.True(set.IsEmpty);
            Assert.Null(set.PeekBottom());
            Assert.Throws<InvalidOperationException>(() => set.DrawTop());
            Assert.Throws<ArgumentNullException>(() => set.Add(null!));
            Assert.Throws<ArgumentNullException>(() => set.AddRange(null!));

            set.FillStandard36();
            Assert.Equal(36, set.Count);
            Assert.NotNull(set.PeekBottom());
            Assert.True(set.HasRank(CardRank.Ace));

            set.Shuffle(new Random(42));
            Card top = set.DrawTop();
            Assert.Equal(35, set.Count);
            Assert.False(set.Contains(top));

            set.AddRange(new[] { top });
            Assert.True(set.Contains(top));
            Assert.True(set.Remove(top));
            set.Clear();
            Assert.True(set.IsEmpty);
        }

        [Fact]
        public void Player_And_TablePair_WorkCorrectly()
        {
            Assert.Throws<ArgumentException>(() => new Player(""));
            Assert.Throws<ArgumentException>(() => new Player("   "));

            Player player = new Player("Іван", isBot: false);
            Assert.Equal("Іван", player.Name);
            Assert.False(player.IsBot);
            Assert.Contains("Іван", player.ToString());

            Assert.Throws<ArgumentNullException>(() => new TablePair(null!));
            TablePair pair = new TablePair(new Card(CardSuit.Diamonds, CardRank.Seven));
            Assert.False(pair.IsBeaten);
            Assert.Contains("не побита", pair.ToString());
            Assert.Throws<ArgumentNullException>(() => pair.BeatWith(null!));

            pair.BeatWith(new Card(CardSuit.Diamonds, CardRank.Ten));
            Assert.True(pair.IsBeaten);
            Assert.Contains("<-", pair.ToString());
        }
    }
}
