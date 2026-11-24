using System;
using System.Collections.Generic;
using NUnit.Framework;
using MemoryGame.GameLogic;

namespace MemoryGame.GameLogic.Tests
{
    public class GameRulesTests
    {
        private sealed class FakeClock : IClock
        {
            public double Now { get; set; }
        }

        private static GameRules MakeRules(out FakeClock clock)
        {
            clock = new FakeClock();
            return new GameRules(new Deck(7), clock);
        }

        [Test]
        public void Flip_FirstThenMatch_BothStayFaceUp()
        {
            var rules = MakeRules(out var clock);
            var a = rules.Cards[0];
            CardModel b = null;
            foreach (var c in rules.Cards) if (c != a && c.Matches(a)) { b = c; break; }

            rules.RequestFlip(a);            // AC-02: 1枚目
            rules.RequestFlip(b);            // 2枚目で判定
            Assert.That(a.State, Is.EqualTo(CardState.Matched));
            Assert.That(b.State, Is.EqualTo(CardState.Matched));
            Assert.That(rules.Moves, Is.EqualTo(1));
        }

        [Test]
        public void Mismatch_ReturnsFaceDownAfterAboutOneSecond()
        {
            var rules = MakeRules(out var clock);
            var a = rules.Cards[0];
            CardModel b = null;
            foreach (var c in rules.Cards) if (c != a && !c.Matches(a)) { b = c; break; }

            rules.RequestFlip(a); rules.RequestFlip(b);
            Assert.That(a.State, Is.EqualTo(CardState.FaceUp));
            Assert.That(b.State, Is.EqualTo(CardState.FaceUp));

            clock.Now += 0.9; rules.Tick();
            Assert.That(a.State, Is.EqualTo(CardState.FaceUp));  // AC-03: 約1秒は表のまま
            clock.Now += 0.2; rules.Tick();
            Assert.That(a.State, Is.EqualTo(CardState.FaceDown));
            Assert.That(b.State, Is.EqualTo(CardState.FaceDown));
        }

        [Test]
        public void MatchedCard_IgnoresFlip()
        {
            var rules = MakeRules(out var clock);
            var a = rules.Cards[0];
            CardModel b = null;
            foreach (var c in rules.Cards) if (c != a && c.Matches(a)) { b = c; break; }
            rules.RequestFlip(a); rules.RequestFlip(b);
            rules.RequestFlip(a);            // AC-04: 確定済みは反応しない
            Assert.That(a.State, Is.EqualTo(CardState.Matched));
            Assert.That(rules.Moves, Is.EqualTo(1));
        }

        [Test]
        public void SameCardTwice_NotTreatedAsSecondCard()
        {
            var rules = MakeRules(out var clock);
            var a = rules.Cards[0];
            rules.RequestFlip(a);
            rules.RequestFlip(a);            // AC-06
            Assert.That(rules.Moves, Is.EqualTo(0));
        }

        [Test]
        public void ThirdFlipWhilePending_Ignored()
        {
            var rules = MakeRules(out var clock);
            var a = rules.Cards[0];
            CardModel b = null;
            foreach (var c in rules.Cards) if (c != a && !c.Matches(a)) { b = c; break; }
            rules.RequestFlip(a); rules.RequestFlip(b);
            CardModel d = null;
            foreach (var c in rules.Cards) if (c.State == CardState.FaceDown) { d = c; break; }
            rules.RequestFlip(d);            // R3: 判定待ち中は3枚目をめくれない
            Assert.That(d.State, Is.EqualTo(CardState.FaceDown));
            clock.Now += 1.1; rules.Tick();  // 元のペアは正しく裏へ戻る
            Assert.That(a.State, Is.EqualTo(CardState.FaceDown));
            Assert.That(b.State, Is.EqualTo(CardState.FaceDown));
            rules.RequestFlip(d);            // 判定完了後はめくれる
            Assert.That(d.State, Is.EqualTo(CardState.FaceUp));
        }

        [Test]
        public void AllPairsMatch_Cleared()
        {
            var rules = MakeRules(out var clock);
            // 番ごとに4枚＝2ペア。全24ペアを揃える。
            var byNumber = new Dictionary<int, List<CardModel>>();
            foreach (var c in rules.Cards)
            {
                if (!byNumber.TryGetValue(c.Number, out var list))
                    byNumber[c.Number] = list = new List<CardModel>();
                list.Add(c);
            }
            foreach (var list in byNumber.Values)
            {
                rules.RequestFlip(list[0]); rules.RequestFlip(list[1]);
                rules.RequestFlip(list[2]); rules.RequestFlip(list[3]);
            }
            Assert.That(rules.Cleared, Is.True);   // AC-05
            Assert.That(rules.Moves, Is.EqualTo(24));
        }

        [Test]
        public void Replay_ResetsStateAndShuffles()
        {
            var rules = MakeRules(out var clock);
            clock.Now += 10;
            var a = rules.Cards[0];
            CardModel b = null;
            foreach (var c in rules.Cards) if (c != a && c.Matches(a)) { b = c; break; }
            rules.RequestFlip(a); rules.RequestFlip(b);
            var before = new HashSet<int>();
            foreach (var c in rules.Cards) before.Add(c.GridIndex);

            rules.Replay();                    // R9
            Assert.That(rules.Moves, Is.EqualTo(0));
            Assert.That(rules.Elapsed, Is.EqualTo(0));
            Assert.That(rules.Cleared, Is.False);
            foreach (var c in rules.Cards) Assert.That(c.State, Is.EqualTo(CardState.FaceDown));
        }
    }
}
