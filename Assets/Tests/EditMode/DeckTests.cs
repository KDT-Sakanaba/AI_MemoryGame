using System.Collections.Generic;
using NUnit.Framework;
using MemoryGame.GameLogic;

namespace MemoryGame.GameLogic.Tests
{
    public class DeckTests
    {
        [Test]
        public void Deck_Has48Cards_24Pairs_AllFaceDown()
        {
            var deck = new Deck(42);
            Assert.That(deck.Cards.Count, Is.EqualTo(48));
            var counts = new Dictionary<int, int>();
            foreach (var c in deck.Cards)
            {
                Assert.That(c.State, Is.EqualTo(CardState.FaceDown)); // AC-01: すべて裏面から始まる
                Assert.That(c.Number, Is.InRange(1, 12));
                counts[c.Number] = counts.GetValueOrDefault(c.Number) + 1;
            }
            Assert.That(counts.Count, Is.EqualTo(12)); // 12番まで、各1枚×4種
            foreach (var kv in counts) Assert.That(kv.Value, Is.EqualTo(4)); // 同じ番4枚＝2ペア/番、計24ペア
        }

        [Test]
        public void Shuffle_AssignsUniqueGridPositions()
        {
            var deck = new Deck(1);
            var seen = new HashSet<int>();
            foreach (var c in deck.Cards) seen.Add(c.GridIndex);
            Assert.That(seen.Count, Is.EqualTo(48));
        }
    }
}
