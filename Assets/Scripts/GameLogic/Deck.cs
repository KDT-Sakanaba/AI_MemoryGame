using System;
using System.Collections.Generic;

namespace MemoryGame.GameLogic
{
    /// <summary>48枚（4種×12番＝24ペア）の盤面。Fisher-Yates シャッフル、6×8グリッド。</summary>
    public sealed class Deck
    {
        public const int Suits = 4;
        public const int MaxNumber = 12; // 本来13番までだが1つ少ない
        public const int Rows = 6;
        public const int Columns = 8;

        private readonly List<CardModel> cards;

        public IReadOnlyList<CardModel> Cards => cards;

        public Deck(int seed = 0)
        {
            cards = new List<CardModel>(Suits * MaxNumber);
            for (int s = 0; s < Suits; s++)
                for (int n = 1; n <= MaxNumber; n++)
                    cards.Add(new CardModel(s, n, 0)); // 普通のトランプの12番まで（Kなし）、各1枚
            Shuffle(seed);
        }

        public void Shuffle(int seed)
        {
            var rng = new Random(seed);
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
            for (int i = 0; i < cards.Count; i++) cards[i].GridIndex = i;
        }
    }
}
