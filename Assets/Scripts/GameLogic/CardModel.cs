namespace MemoryGame.GameLogic
{
    public enum CardState { FaceDown, FaceUp, Matched }

    /// <summary>1枚分のカード状態。View 非依存。</summary>
    public sealed class CardModel
    {
        public int Suit { get; }        // 0..3（4種類）
        public int Number { get; }      // 1..12（本来13番までだが1つ少ない）
        public CardState State { get; private set; }
        public int GridIndex { get; internal set; } // 0..47（6×8）

        public CardModel(int suit, int number, int gridIndex)
        {
            Suit = suit;
            Number = number;
            GridIndex = gridIndex;
            State = CardState.FaceDown;
        }

        public bool Matches(CardModel other) => Number == other.Number; // ペア判定は数字（番）のみ

        public void Flip()
        {
            if (State == CardState.FaceDown) State = CardState.FaceUp;
        }

        public void Match() => State = CardState.Matched;

        public void Reset() => State = CardState.FaceDown;
    }
}
