using System;
using System.Collections.Generic;

namespace MemoryGame.GameLogic
{
    /// <summary>神経衰弱の判定ルール。View 非依存。遅延は注入された IClock で判定する。</summary>
    public sealed class GameRules
    {
        public const double MismatchDelaySeconds = 1.0;

        private readonly Deck deck;
        private readonly IClock clock;
        private CardModel first;
        private CardModel pendingA;
        private CardModel pendingB;
        private double returnAt;
        private double startTime;

        public int Moves { get; private set; }
        public bool Cleared { get; private set; }
        public IReadOnlyList<CardModel> Cards => deck.Cards;
        public double Elapsed => clock.Now - startTime;

        public event Action<CardModel> StateChanged;

        public GameRules(Deck deck, IClock clock)
        {
            this.deck = deck;
            this.clock = clock;
            startTime = clock.Now;
        }

        public void RequestFlip(CardModel card)
        {
            if (Cleared || card.State != CardState.FaceDown) return; // 確定済み・既に表（同一カードの2回目）は無視
            if (pendingA != null) return; // R3: 不一致ペアの判定待ち中は3枚目をめくれない

            card.Flip();
            StateChanged?.Invoke(card);

            if (first == null) { first = card; return; }

            Moves++;
            if (first.Matches(card))
            {
                first.Match(); card.Match();
                StateChanged?.Invoke(first); StateChanged?.Invoke(card);
                first = null;
                if (Moves == deck.Cards.Count / 2) Cleared = true;
            }
            else
            {
                pendingA = first; pendingB = card;
                returnAt = clock.Now + MismatchDelaySeconds;
                first = null;
            }
        }

        /// <summary>不一致ペアの「約1秒後に裏へ」を処理する。毎フレーム呼ぶ。</summary>
        public void Tick()
        {
            if (pendingA == null || clock.Now < returnAt) return;
            pendingA.Reset(); pendingB.Reset();
            StateChanged?.Invoke(pendingA); StateChanged?.Invoke(pendingB);
            pendingA = null; pendingB = null;
        }

        public void Replay()
        {
            foreach (var c in deck.Cards) c.Reset();
            deck.Shuffle(Environment.TickCount);
            Moves = 0; Cleared = false; first = null; pendingA = null; pendingB = null;
            startTime = clock.Now;
            foreach (var c in deck.Cards) StateChanged?.Invoke(c);
        }
    }
}
